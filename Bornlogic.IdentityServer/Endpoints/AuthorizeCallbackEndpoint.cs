


using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Endpoints.Results;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Hosting;
using Bornlogic.IdentityServer.Models.Messages;
using Bornlogic.IdentityServer.ResponseHandling;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Stores;
using Bornlogic.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Net;

namespace Bornlogic.IdentityServer.Endpoints
{
    internal class AuthorizeCallbackEndpoint : AuthorizeEndpointBase
    {
        private readonly IConsentMessageStore _consentResponseStore;
        private readonly IBusinessSelectMessageStore _businessSelectMessageStore;
        private readonly IAuthorizationParametersMessageStore _authorizationParametersMessageStore;

        public AuthorizeCallbackEndpoint(
            IEventService events,
            ILogger<AuthorizeCallbackEndpoint> logger,
            IdentityServerOptions options,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeResponseGenerator authorizeResponseGenerator,
            IUserSession userSession,
            IConsentMessageStore consentResponseStore,
            IBusinessSelectMessageStore businessSelectMessageStore,
            IAuthorizationParametersMessageStore authorizationParametersMessageStore = null)
            : base(events, logger, options, validator, interactionGenerator, authorizeResponseGenerator, userSession)
        {
            _consentResponseStore = consentResponseStore;
            _businessSelectMessageStore = businessSelectMessageStore;
            _authorizationParametersMessageStore = authorizationParametersMessageStore;
        }

        public override async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                Logger.LogWarning("Invalid HTTP method for authorize endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            Logger.LogDebug("Start authorize callback request");

            var parameters = context.Request.Query.AsNameValueCollection();
            if (_authorizationParametersMessageStore != null)
            {
                var messageStoreId = parameters[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
                var entry = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);
                parameters = entry?.Data.FromFullDictionary() ?? new NameValueCollection();

                await _authorizationParametersMessageStore.DeleteAsync(messageStoreId);
            }

            var user = await UserSession.GetUserAsync();
            var consentRequest = new ConsentRequest(parameters, user?.GetSubjectId());
            var consent = await _consentResponseStore.ReadAsync(consentRequest.Id);
            
            if (consent != null && consent.Data == null)
            {
                return await CreateErrorResultAsync("consent message is missing data");
            }

            var businessSelectRequest = new BusinessSelectRequest(parameters, user?.GetSubjectId());
            var businessSelect= await _businessSelectMessageStore.ReadAsync(businessSelectRequest.Id);

            var clearSessionValues = false;

            try
            {
                var result = await ProcessAuthorizeRequestAsync(parameters, user, consent?.Data, businessSelect?.Data);

                clearSessionValues = result is AuthorizeResult;

                Logger.LogTrace("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

                return result;
            }
            finally
            {
                if (consent != null && clearSessionValues)
                {
                    await _consentResponseStore.DeleteAsync(consentRequest.Id);
                }

                if (businessSelect != null && clearSessionValues)
                {
                    await _businessSelectMessageStore.DeleteAsync(businessSelectRequest.Id);
                }
            }
        }
    }
}
