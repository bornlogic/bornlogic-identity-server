using System.Collections.Specialized;
using System.Security.Claims;
using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Logging.Models;
using Bornlogic.IdentityServer.Models.Contexts;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Stores;
using Bornlogic.IdentityServer.Validation.Models;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Validation.Default
{
    /// <summary>
    /// Validates requests to the end session endpoint.
    /// </summary>
    public class EndSessionRequestValidator : IEndSessionRequestValidator
    {
        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        ///  The IdentityServer options.
        /// </summary>
        protected readonly IdentityServerOptions Options;

        /// <summary>
        /// The token validator.
        /// </summary>
        protected readonly ITokenValidator TokenValidator;

        /// <summary>
        /// The URI validator.
        /// </summary>
        protected readonly IRedirectUriValidator UriValidator;

        /// <summary>
        /// The user session service.
        /// </summary>
        protected readonly IUserSession UserSession;

        /// <summary>
        /// The logout notification service.
        /// </summary>
        public ILogoutNotificationService LogoutNotificationService { get; }

        /// <summary>
        /// The end session message store.
        /// </summary>
        protected readonly IMessageStore<LogoutNotificationContext> EndSessionMessageStore;

        /// <summary>
        /// The HTTP context accessor.
        /// </summary>
        protected readonly IHttpContextAccessor Context;

        /// <summary>
        /// Creates a new instance of the EndSessionRequestValidator.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="tokenValidator"></param>
        /// <param name="uriValidator"></param>
        /// <param name="userSession"></param>
        /// <param name="logoutNotificationService"></param>
        /// <param name="endSessionMessageStore"></param>
        /// <param name="logger"></param>
        public EndSessionRequestValidator(
            IHttpContextAccessor context,
            IdentityServerOptions options,
            ITokenValidator tokenValidator,
            IRedirectUriValidator uriValidator,
            IUserSession userSession,
            ILogoutNotificationService logoutNotificationService,
            IMessageStore<LogoutNotificationContext> endSessionMessageStore,
            ILogger<EndSessionRequestValidator> logger)
        {
            Context = context;
            Options = options;
            TokenValidator = tokenValidator;
            UriValidator = uriValidator;
            UserSession = userSession;
            LogoutNotificationService = logoutNotificationService;
            EndSessionMessageStore = endSessionMessageStore;
            Logger = logger;
        }

        /// <inheritdoc />
        public async Task<EndSessionValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject)
        {
            Logger.LogDebug("Start end session request validation");

            var isAuthenticated = subject.IsAuthenticated();

            if (!isAuthenticated && Options.Authentication.RequireAuthenticatedUserForSignOutMessage)
            {
                return Invalid("user_is_anonymous_ignore_parameters");
            }

            var validatedRequest = new ValidatedEndSessionRequest
            {
                Raw = parameters
            };

            var idTokenHint = parameters.Get(OidcConstants.EndSessionRequest.IdTokenHint);
            if (idTokenHint.IsPresent())
            {
                // validate id_token - no need to validate token life time
                var tokenValidationResult = await TokenValidator.ValidateIdentityTokenAsync(idTokenHint, null, false);
                if (tokenValidationResult.IsError)
                {
                    return Invalid("error_validating_id_token_hint", validatedRequest);
                }

                validatedRequest.Client = tokenValidationResult.Client;

                // validate sub claim against currently logged on user
                var subClaim = tokenValidationResult.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);
                if (subClaim != null && isAuthenticated)
                {
                    if (subject.GetSubjectId() != subClaim.Value)
                    {
                        return Invalid("current_user_does_not_match_identity_token", validatedRequest);
                    }

                    validatedRequest.Subject = subject;
                    validatedRequest.SessionId = await UserSession.GetSessionIdAsync();
                    validatedRequest.ClientIds = await UserSession.GetClientListAsync();
                }

                var redirectUri = parameters.Get(OidcConstants.EndSessionRequest.PostLogoutRedirectUri);
                if (redirectUri.IsPresent())
                {
                    if (await UriValidator.IsPostLogoutRedirectUriValidAsync(redirectUri, validatedRequest.Client))
                    {
                        validatedRequest.PostLogOutUri = redirectUri;
                    }
                    else
                    {
                        Logger.LogWarning("Invalid PostLogoutRedirectUri: {postLogoutRedirectUri}", redirectUri);
                    }
                }

                if (validatedRequest.PostLogOutUri != null)
                {
                    var state = parameters.Get(OidcConstants.EndSessionRequest.State);
                    if (state.IsPresent())
                    {
                        validatedRequest.State = state;
                    }
                }
            }
            else
            {
                // no id_token to authenticate the client, but we do have a user and a user session
                validatedRequest.Subject = subject;
                validatedRequest.SessionId = await UserSession.GetSessionIdAsync();
                validatedRequest.ClientIds = await UserSession.GetClientListAsync();
            }

            LogSuccess(validatedRequest);

            return new EndSessionValidationResult
            {
                ValidatedRequest = validatedRequest,
                IsError = false
            };
        }

        /// <summary>
        /// Creates a result that indicates an error.
        /// </summary>
        /// <param name="subError"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual EndSessionValidationResult Invalid(string subError, ValidatedEndSessionRequest request = null)
        {
            subError = "end_sessions_" + subError;
            if (request != null)
            {
                var log = new EndSessionRequestValidationLog(request);
                Logger.LogInformation(subError + Environment.NewLine + "{@details}", log);
            }
            else
            {
                Logger.LogInformation(subError);
            }

            return new EndSessionValidationResult
            {
                IsError = true,
                Error = "invalid_request",
                SubError = subError
            };
        }

        /// <summary>
        /// Logs a success result.
        /// </summary>
        /// <param name="request"></param>
        protected virtual void LogSuccess(ValidatedEndSessionRequest request)
        {
            var log = new EndSessionRequestValidationLog(request);
            Logger.LogInformation("End session request validation success" + Environment.NewLine + "{@details}", log);
        }

        /// <inheritdoc />
        public async Task<EndSessionCallbackValidationResult> ValidateCallbackAsync(NameValueCollection parameters)
        {
            var result = new EndSessionCallbackValidationResult
            {
                IsError = true
            };

            var endSessionId = parameters[Constants.UIConstants.DefaultRoutePathParams.EndSessionCallback];
            var endSessionMessage = await EndSessionMessageStore.ReadAsync(endSessionId);
            if (endSessionMessage?.Data?.ClientIds?.Any() == true)
            {
                result.IsError = false;
                result.FrontChannelLogoutUrls = await LogoutNotificationService.GetFrontChannelLogoutNotificationsUrlsAsync(endSessionMessage.Data);
            }
            else
            {
                result.Error = "Failed to read end session callback message";
            }

            return result;
        }
    }
}