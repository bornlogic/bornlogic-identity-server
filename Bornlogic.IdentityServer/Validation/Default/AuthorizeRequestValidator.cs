// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Security.Claims;
using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Logging.Models;
using Bornlogic.IdentityServer.Models.Messages;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Services;
using Bornlogic.IdentityServer.Storage.Stores;
using Bornlogic.IdentityServer.Stores;
using Bornlogic.IdentityServer.Validation.Contexts;
using Bornlogic.IdentityServer.Validation.Models;
using IdentityModel;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Validation.Default
{
    internal class AuthorizeRequestValidator : IAuthorizeRequestValidator
    {
        private readonly IdentityServerOptions _options;
        private readonly IClientStore _clients;
        private readonly ICustomAuthorizeRequestValidator _customValidator;
        private readonly IRedirectUriValidator _uriValidator;
        private readonly IResourceValidator _resourceValidator;
        private readonly IUserSession _userSession;
        private readonly JwtRequestValidator _jwtRequestValidator;
        private readonly IJwtRequestUriHttpClient _jwtRequestUriHttpClient;
        private readonly ILogger _logger;
        private readonly IUserEmailStore _userEmailStore;
        private readonly IUserManagerService _userManagerService;
        private readonly IClientUserRoleService _clientUserRoleService;
        private readonly IBusinessSelectMessageStore _businessSelectMessageStore;

        private readonly ResponseTypeEqualityComparer
            _responseTypeEqualityComparer = new ResponseTypeEqualityComparer();

        public AuthorizeRequestValidator(
            IdentityServerOptions options,
            IClientStore clients,
            ICustomAuthorizeRequestValidator customValidator,
            IRedirectUriValidator uriValidator,
            IResourceValidator resourceValidator,
            IUserSession userSession,
            JwtRequestValidator jwtRequestValidator,
            IJwtRequestUriHttpClient jwtRequestUriHttpClient,
            ILogger<AuthorizeRequestValidator> logger,
            IUserEmailStore userEmailStore,
            IUserManagerService userManagerService,
            IClientUserRoleService clientUserRoleService,
            IBusinessSelectMessageStore businessSelectMessageStore
            )
        {
            _options = options;
            _clients = clients;
            _customValidator = customValidator;
            _uriValidator = uriValidator;
            _resourceValidator = resourceValidator;
            _jwtRequestValidator = jwtRequestValidator;
            _userSession = userSession;
            _jwtRequestUriHttpClient = jwtRequestUriHttpClient;
            _logger = logger;
            _userEmailStore = userEmailStore;
            _userManagerService = userManagerService;
            _clientUserRoleService = clientUserRoleService;
            _businessSelectMessageStore = businessSelectMessageStore;
        }

        public async Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject = null)
        {
            _logger.LogDebug("Start authorize request protocol validation");

            var request = new ValidatedAuthorizeRequest
            {
                Options = _options,
                Subject = subject ?? Principal.Anonymous,
                Raw = parameters ?? throw new ArgumentNullException(nameof(parameters))
            };

            // load client_id
            // client_id must always be present on the request
            var loadClientResult = await LoadClientAsync(request);
            if (loadClientResult.IsError)
            {
                return loadClientResult;
            }

            if (subject?.Claims != null && subject.HasClaim("email_verified", "false"))
            {
                var emailIsConfirmed = await _userEmailStore.UserEmailIsConfirmedAsync(subject.GetSubjectId());

                if (emailIsConfirmed)
                {
                    await _userManagerService.UpsertClaim(subject, new Claim("email_verified", "true"));
                }
                else if (request.Client.AcceptVerifiedUsersOnly)
                {
                    return Invalid(request, "Email is not verified", "The user must have a verified email");
                }
            }

            // load request object
            var roLoadResult = await LoadRequestObjectAsync(request);
            if (roLoadResult.IsError)
            {
                return roLoadResult;
            }

            // validate request object
            var roValidationResult = await ValidateRequestObjectAsync(request);
            if (roValidationResult.IsError)
            {
                return roValidationResult;
            }

            // validate client_id and redirect_uri
            var clientResult = await ValidateClientAsync(request);
            if (clientResult.IsError)
            {
                return clientResult;
            }

            // state, response_type, response_mode
            var mandatoryResult = ValidateCoreParameters(request);
            if (mandatoryResult.IsError)
            {
                return mandatoryResult;
            }

            // scope, scope restrictions and plausability
            var scopeResult = await ValidateScopeAsync(request);
            if (scopeResult.IsError)
            {
                return scopeResult;
            }

            // nonce, prompt, acr_values, login_hint etc.
            var optionalResult = await ValidateOptionalParametersAsync(request);
            if (optionalResult.IsError)
            {
                return optionalResult;
            }

            // custom validator
            _logger.LogDebug("Calling into custom validator: {type}", _customValidator.GetType().FullName);
            var context = new CustomAuthorizeRequestValidationContext
            {
                Result = new AuthorizeRequestValidationResult(request)
            };
            await _customValidator.ValidateAsync(context);

            var customResult = context.Result;
            if (customResult.IsError)
            {
                LogError("Error in custom validation", customResult.Error, request);
                return Invalid(request, customResult.Error, customResult.SubError);
            }

            _logger.LogTrace("Authorize request protocol validation successful");

            return Valid(request);
        }

        private async Task<AuthorizeRequestValidationResult> LoadRequestObjectAsync(ValidatedAuthorizeRequest request)
        {
            var jwtRequest = request.Raw.Get(OidcConstants.AuthorizeRequest.Request);
            var jwtRequestUri = request.Raw.Get(OidcConstants.AuthorizeRequest.RequestUri);

            if (jwtRequest.IsPresent() && jwtRequestUri.IsPresent())
            {
                LogError("Both request and request_uri are present", request);
                return Invalid(request, subError: "only_one_request_parameter_is_allowed");
            }

            if (_options.Endpoints.EnableJwtRequestUri)
            {
                if (jwtRequestUri.IsPresent())
                {
                    // 512 is from the spec
                    if (jwtRequestUri.Length > 512)
                    {
                        LogError("request_uri is too long", request);
                        return Invalid(request, error: OidcConstants.AuthorizeErrors.InvalidRequestUri, subError: "request_uri_too_long");
                    }

                    var jwt = await _jwtRequestUriHttpClient.GetJwtAsync(jwtRequestUri, request.Client);
                    if (jwt.IsMissing())
                    {
                        LogError("no value returned from request_uri", request);
                        return Invalid(request, error: OidcConstants.AuthorizeErrors.InvalidRequestUri, subError: "request_uri_empty");
                    }

                    jwtRequest = jwt;
                }
            }
            else if (jwtRequestUri.IsPresent())
            {
                LogError("request_uri present but config prohibits", request);
                return Invalid(request, error: OidcConstants.AuthorizeErrors.RequestUriNotSupported);
            }

            // check length restrictions
            if (jwtRequest.IsPresent())
            {
                if (jwtRequest.Length >= _options.InputLengthRestrictions.Jwt)
                {
                    LogError("request value is too long", request);
                    return Invalid(request, error: OidcConstants.AuthorizeErrors.InvalidRequestObject, subError: "invalid_request_value");
                }
            }

            request.RequestObject = jwtRequest;
            return Valid(request);
        }

        private async Task<AuthorizeRequestValidationResult> LoadClientAsync(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // client_id must be present
            /////////////////////////////////////////////////////////
            var clientId = request.Raw.Get(OidcConstants.AuthorizeRequest.ClientId);

            if (clientId.IsMissingOrTooLong(_options.InputLengthRestrictions.ClientId))
            {
                LogError("client_id is missing or too long", request);
                return Invalid(request, subError: "invalid_client_id");
            }

            request.ClientId = clientId;

            //////////////////////////////////////////////////////////
            // check for valid client
            //////////////////////////////////////////////////////////
            
            var subjectIdOrDefault = request.Subject?.GetSubjectIdOrDefault();

            var client = string.IsNullOrEmpty(subjectIdOrDefault)
                ? await _clients.FindClientByIdAsync(request.ClientId)
                : await _clients.FindEnabledClientByIdAsync(request.ClientId, _clientUserRoleService,
                    subjectIdOrDefault);

            if (client == null)
            {
                LogError("Unknown client or not enabled", request.ClientId, request);
                return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, "unknown_or_not_enabled");
            }

            request.SetClient(client);

            return Valid(request);
        }

        private async Task<AuthorizeRequestValidationResult> ValidateRequestObjectAsync(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // validate request object
            /////////////////////////////////////////////////////////
            if (request.RequestObject.IsPresent())
            {
                // validate the request JWT for this client
                var jwtRequestValidationResult = await _jwtRequestValidator.ValidateAsync(request.Client, request.RequestObject);
                if (jwtRequestValidationResult.IsError)
                {
                    LogError("request JWT validation failure", request);
                    return Invalid(request, error: OidcConstants.AuthorizeErrors.InvalidRequestObject, subError: "invalid_jwt_request");
                }

                // validate response_type match
                var responseType = request.Raw.Get(OidcConstants.AuthorizeRequest.ResponseType);
                if (responseType != null)
                {
                    if (jwtRequestValidationResult.Payload.TryGetValue(OidcConstants.AuthorizeRequest.ResponseType, out var payloadResponseType))
                    {
                        if (payloadResponseType != responseType)
                        {
                            LogError("response_type in JWT payload does not match response_type in request", request);
                            return Invalid(request, subError: "invalid_jwt_request");
                        }
                    }
                }

                // validate client_id mismatch
                if (jwtRequestValidationResult.Payload.TryGetValue(OidcConstants.AuthorizeRequest.ClientId, out var payloadClientId))
                {
                    if (!string.Equals(request.Client.ClientId, payloadClientId, StringComparison.Ordinal))
                    {
                        LogError("client_id in JWT payload does not match client_id in request", request);
                        return Invalid(request, subError: "invalid_jwt_request");
                    }
                }
                else
                {
                    LogError("client_id is missing in JWT payload", request);
                    return Invalid(request, error: OidcConstants.AuthorizeErrors.InvalidRequestObject, subError: "invalid_jwt_request");
                }

                var ignoreKeys = new[]
                {
                    JwtClaimTypes.Issuer,
                    JwtClaimTypes.Audience
                };

                // merge jwt payload values into original request parameters
                foreach (var key in jwtRequestValidationResult.Payload.Keys)
                {
                    if (ignoreKeys.Contains(key)) continue;

                    var value = jwtRequestValidationResult.Payload[key];

                    var qsValue = request.Raw.Get(key);
                    if (qsValue != null)
                    {
                        if (!string.Equals(value, qsValue, StringComparison.Ordinal))
                        {
                            LogError("parameter mismatch between request object and query string parameter.", request);
                            return Invalid(request, subError: "jwt_request_parameter_mismatch");
                        }
                    }

                    request.Raw.Set(key, value);
                }

                request.RequestObjectValues = jwtRequestValidationResult.Payload;
            }

            return Valid(request);
        }

        private async Task<AuthorizeRequestValidationResult> ValidateClientAsync(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // check request object requirement
            //////////////////////////////////////////////////////////
            if (request.Client.RequireRequestObject)
            {
                if (!request.RequestObjectValues.Any())
                {
                    return Invalid(request, subError: "missing_request_or_request_uri");
                }
            }

            //////////////////////////////////////////////////////////
            // redirect_uri must be present, and a valid uri
            //////////////////////////////////////////////////////////
            var redirectUri = request.Raw.Get(OidcConstants.AuthorizeRequest.RedirectUri);

            if (redirectUri.IsMissingOrTooLong(_options.InputLengthRestrictions.RedirectUri))
            {
                LogError("redirect_uri is missing or too long", request);
                return Invalid(request, subError: "invalid_redirect_uri");
            }

            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out _))
            {
                LogError("malformed redirect_uri", redirectUri, request);
                return Invalid(request, subError: "invalid_redirect_uri");
            }

            //////////////////////////////////////////////////////////
            // check if client protocol type is oidc
            //////////////////////////////////////////////////////////
            if (request.Client.ProtocolType != IdentityServerConstants.ProtocolTypes.OpenIdConnect)
            {
                LogError("Invalid protocol type for OIDC authorize endpoint", request.Client.ProtocolType, request);
                return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, subError: "invalid_protocol");
            }

            //////////////////////////////////////////////////////////
            // check if redirect_uri is valid
            //////////////////////////////////////////////////////////
            if (await _uriValidator.IsRedirectUriValidAsync(redirectUri, request.Client) == false)
            {
                LogError("Invalid redirect_uri", redirectUri, request);
                return Invalid(request, OidcConstants.AuthorizeErrors.InvalidRequest, "Invalid redirect_uri");
            }

            request.RedirectUri = redirectUri;

            return Valid(request);
        }

        private AuthorizeRequestValidationResult ValidateCoreParameters(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // check state
            //////////////////////////////////////////////////////////
            var state = request.Raw.Get(OidcConstants.AuthorizeRequest.State);
            if (state.IsPresent())
            {
                request.State = state;
            }

            //////////////////////////////////////////////////////////
            // response_type must be present and supported
            //////////////////////////////////////////////////////////
            var responseType = request.Raw.Get(OidcConstants.AuthorizeRequest.ResponseType);
            if (responseType.IsMissing())
            {
                LogError("Missing response_type", request);
                return Invalid(request, OidcConstants.AuthorizeErrors.UnsupportedResponseType, "Missing response_type");
            }

            // The responseType may come in in an unconventional order.
            // Use an IEqualityComparer that doesn't care about the order of multiple values.
            // Per https://tools.ietf.org/html/rfc6749#section-3.1.1 -
            // 'Extension response types MAY contain a space-delimited (%x20) list of
            // values, where the order of values does not matter (e.g., response
            // type "a b" is the same as "b a").'
            // http://openid.net/specs/oauth-v2-multiple-response-types-1_0-03.html#terminology -
            // 'If a response type contains one of more space characters (%20), it is compared
            // as a space-delimited list of values in which the order of values does not matter.'
            if (!Constants.SupportedResponseTypes.Contains(responseType, _responseTypeEqualityComparer))
            {
                LogError("Response type not supported", responseType, request);
                return Invalid(request, OidcConstants.AuthorizeErrors.UnsupportedResponseType, "Response type not supported");
            }

            // Even though the responseType may have come in in an unconventional order,
            // we still need the request's ResponseType property to be set to the
            // conventional, supported response type.
            request.ResponseType = Constants.SupportedResponseTypes.First(
                supportedResponseType => _responseTypeEqualityComparer.Equals(supportedResponseType, responseType));

            //////////////////////////////////////////////////////////
            // match response_type to grant type
            //////////////////////////////////////////////////////////
            request.GrantType = Constants.ResponseTypeToGrantTypeMapping[request.ResponseType];

            // set default response mode for flow; this is needed for any client error processing below
            request.ResponseMode = Constants.AllowedResponseModesForGrantType[request.GrantType].First();

            //////////////////////////////////////////////////////////
            // check if flow is allowed at authorize endpoint
            //////////////////////////////////////////////////////////
            if (!Constants.AllowedGrantTypesForAuthorizeEndpoint.Contains(request.GrantType))
            {
                LogError("Invalid grant type", request.GrantType, request);
                return Invalid(request, subError: "invalid_response_type");
            }

            //////////////////////////////////////////////////////////
            // check if PKCE is required and validate parameters
            //////////////////////////////////////////////////////////
            if (request.GrantType == GrantType.AuthorizationCode || request.GrantType == GrantType.Hybrid)
            {
                _logger.LogDebug("Checking for PKCE parameters");

                /////////////////////////////////////////////////////////////////////////////
                // validate code_challenge and code_challenge_method
                /////////////////////////////////////////////////////////////////////////////
                var proofKeyResult = ValidatePkceParameters(request);

                if (proofKeyResult.IsError)
                {
                    return proofKeyResult;
                }
            }

            //////////////////////////////////////////////////////////
            // check response_mode parameter and set response_mode
            //////////////////////////////////////////////////////////

            // check if response_mode parameter is present and valid
            var responseMode = request.Raw.Get(OidcConstants.AuthorizeRequest.ResponseMode);
            if (responseMode.IsPresent())
            {
                if (Constants.SupportedResponseModes.Contains(responseMode))
                {
                    if (Constants.AllowedResponseModesForGrantType[request.GrantType].Contains(responseMode))
                    {
                        request.ResponseMode = responseMode;
                    }
                    else
                    {
                        LogError("Invalid response_mode for response_type", responseMode, request);
                        return Invalid(request, OidcConstants.AuthorizeErrors.InvalidRequest, subError: "invalid_response_mode_for_response_type");
                    }
                }
                else
                {
                    LogError("Unsupported response_mode", responseMode, request);
                    return Invalid(request, OidcConstants.AuthorizeErrors.UnsupportedResponseType, subError: "invalid_response_mode");
                }
            }


            //////////////////////////////////////////////////////////
            // check if grant type is allowed for client
            //////////////////////////////////////////////////////////
            if (!request.Client.AllowedGrantTypes.Contains(request.GrantType))
            {
                LogError("Invalid grant type for client", request.GrantType, request);
                return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, "invalid_grant_type_for_client");
            }

            //////////////////////////////////////////////////////////
            // check if response type contains an access token,
            // and if client is allowed to request access token via browser
            //////////////////////////////////////////////////////////
            var responseTypes = responseType.FromSpaceSeparatedString();
            if (responseTypes.Contains(OidcConstants.ResponseTypes.Token))
            {
                if (!request.Client.AllowAccessTokensViaBrowser)
                {
                    LogError("Client requested access token - but client is not configured to receive access tokens via browser", request);
                    return Invalid(request, subError: "browser_access_tokens_disabled_for_client");
                }
            }

            return Valid(request);
        }

        private AuthorizeRequestValidationResult ValidatePkceParameters(ValidatedAuthorizeRequest request)
        {
            var fail = Invalid(request);

            var codeChallenge = request.Raw.Get(OidcConstants.AuthorizeRequest.CodeChallenge);
            if (codeChallenge.IsMissing())
            {
                if (request.Client.RequirePkce)
                {
                    LogError("code_challenge is missing", request);
                    fail.SubError = "code_challenge_required";
                }
                else
                {
                    _logger.LogDebug("No PKCE used.");
                    return Valid(request);
                }

                return fail;
            }

            if (codeChallenge.Length < _options.InputLengthRestrictions.CodeChallengeMinLength ||
                codeChallenge.Length > _options.InputLengthRestrictions.CodeChallengeMaxLength)
            {
                LogError("code_challenge is either too short or too long", request);
                fail.SubError = "invalid_code_challenge";
                return fail;
            }

            request.CodeChallenge = codeChallenge;

            var codeChallengeMethod = request.Raw.Get(OidcConstants.AuthorizeRequest.CodeChallengeMethod);
            if (codeChallengeMethod.IsMissing())
            {
                _logger.LogDebug("Missing code_challenge_method, defaulting to plain");
                codeChallengeMethod = OidcConstants.CodeChallengeMethods.Plain;
            }

            if (!Constants.SupportedCodeChallengeMethods.Contains(codeChallengeMethod))
            {
                LogError("Unsupported code_challenge_method", codeChallengeMethod, request);
                fail.SubError = "transform_algorithm_not_supported";
                return fail;
            }

            // check if plain method is allowed
            if (codeChallengeMethod == OidcConstants.CodeChallengeMethods.Plain)
            {
                if (!request.Client.AllowPlainTextPkce)
                {
                    LogError("code_challenge_method of plain is not allowed", request);
                    fail.SubError = "transform_algorithm_not_supported";
                    return fail;
                }
            }

            request.CodeChallengeMethod = codeChallengeMethod;

            return Valid(request);
        }

        private async Task<AuthorizeRequestValidationResult> ValidateScopeAsync(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // scope must be present
            //////////////////////////////////////////////////////////
            var scope = request.Raw.Get(OidcConstants.AuthorizeRequest.Scope);
            if (scope.IsMissing())
            {
                LogError("scope is missing", request);
                return Invalid(request, subError: "invalid_scope");
            }

            if (scope.Length > _options.InputLengthRestrictions.Scope)
            {
                LogError("scopes too long.", request);
                return Invalid(request, subError: "invalid_scope");
            }

            request.RequestedScopes = scope.FromSpaceSeparatedString().Distinct().ToList();

            if (request.RequestedScopes.Contains(IdentityServerConstants.StandardScopes.OpenId))
            {
                request.IsOpenIdRequest = true;
            }

            //////////////////////////////////////////////////////////
            // check scope vs response_type plausability
            //////////////////////////////////////////////////////////
            var requirement = Constants.ResponseTypeToScopeRequirement[request.ResponseType];
            if (requirement == Constants.ScopeRequirement.Identity ||
                requirement == Constants.ScopeRequirement.IdentityOnly)
            {
                if (request.IsOpenIdRequest == false)
                {
                    LogError("response_type requires the openid scope", request);
                    return Invalid(request, subError: "missing_openid_scope");
                }
            }

            //////////////////////////////////////////////////////////
            // check if scopes are valid/supported and check for resource scopes
            //////////////////////////////////////////////////////////
            var validatedResources = await _resourceValidator.ValidateRequestedResourcesAsync(new ResourceValidationRequest
            {
                Client = request.Client,
                Scopes = request.RequestedScopes,
                RequiredRequestScopes = request.Raw.Get("required_scope")?.Split(' ') ?? Array.Empty<string>()
            });

            if (!validatedResources.Succeeded)
            {
                return Invalid(request, OidcConstants.AuthorizeErrors.InvalidScope);
            }

            if (validatedResources.Resources.IdentityResources.Any() && !request.IsOpenIdRequest)
            {
                LogError("Identity related scope requests, but no openid scope", request);
                return Invalid(request, OidcConstants.AuthorizeErrors.InvalidScope, "missing_openid_scope");
            }

            if (validatedResources.Resources.ApiScopes.Any())
            {
                request.IsApiResourceRequest = true;
            }

            //////////////////////////////////////////////////////////
            // check id vs resource scopes and response types plausability
            //////////////////////////////////////////////////////////
            var responseTypeValidationCheck = true;
            switch (requirement)
            {
                case Constants.ScopeRequirement.Identity:
                    if (!validatedResources.Resources.IdentityResources.Any())
                    {
                        _logger.LogError("Requests for id_token response type must include identity scopes");
                        responseTypeValidationCheck = false;
                    }
                    break;
                case Constants.ScopeRequirement.IdentityOnly:
                    if (!validatedResources.Resources.IdentityResources.Any() || validatedResources.Resources.ApiScopes.Any())
                    {
                        _logger.LogError("Requests for id_token response type only must not include resource scopes");
                        responseTypeValidationCheck = false;
                    }
                    break;
                case Constants.ScopeRequirement.ResourceOnly:
                    if (validatedResources.Resources.IdentityResources.Any() || !validatedResources.Resources.ApiScopes.Any())
                    {
                        _logger.LogError("Requests for token response type only must include resource scopes, but no identity scopes.");
                        responseTypeValidationCheck = false;
                    }
                    break;
            }

            if (!responseTypeValidationCheck)
            {
                return Invalid(request, OidcConstants.AuthorizeErrors.InvalidScope, "invalid_scope_for_response_type");
            }

            request.ValidatedResources = validatedResources;

            return Valid(request);
        }

        private async Task<AuthorizeRequestValidationResult> ValidateOptionalParametersAsync(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // check nonce
            //////////////////////////////////////////////////////////
            var nonce = request.Raw.Get(OidcConstants.AuthorizeRequest.Nonce);
            if (nonce.IsPresent())
            {
                if (nonce.Length > _options.InputLengthRestrictions.Nonce)
                {
                    LogError("Nonce too long", request);
                    return Invalid(request, subError: "invalid_nonce");
                }

                request.Nonce = nonce;
            }
            else
            {
                if (request.GrantType == GrantType.Implicit ||
                    request.GrantType == GrantType.Hybrid)
                {
                    // only openid requests require nonce
                    if (request.IsOpenIdRequest)
                    {
                        LogError("Nonce required for implicit and hybrid flow with openid scope", request);
                        return Invalid(request, subError: "invalid_nonce");
                    }
                }
            }


            //////////////////////////////////////////////////////////
            // check prompt
            //////////////////////////////////////////////////////////
            var prompt = request.Raw.Get(OidcConstants.AuthorizeRequest.Prompt);
            if (prompt.IsPresent())
            {
                var prompts = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (prompts.All(p => Constants.SupportedPromptModes.Contains(p)))
                {
                    if (prompts.Contains(OidcConstants.PromptModes.None) && prompts.Length > 1)
                    {
                        LogError("prompt contains 'none' and other values. 'none' should be used by itself.", request);
                        return Invalid(request, subError: "invalid_prompt");
                    }

                    request.PromptModes = prompts;
                }
                else
                {
                    _logger.LogDebug("Unsupported prompt mode - ignored: " + prompt);
                }
            }

            //////////////////////////////////////////////////////////
            // check ui locales
            //////////////////////////////////////////////////////////
            var uilocales = request.Raw.Get(OidcConstants.AuthorizeRequest.UiLocales);
            if (uilocales.IsPresent())
            {
                if (uilocales.Length > _options.InputLengthRestrictions.UiLocale)
                {
                    LogError("UI locale too long", request);
                    return Invalid(request, subError: "invalid_ui_locales");
                }

                request.UiLocales = uilocales;
            }

            //////////////////////////////////////////////////////////
            // check display
            //////////////////////////////////////////////////////////
            var display = request.Raw.Get(OidcConstants.AuthorizeRequest.Display);
            if (display.IsPresent())
            {
                if (Constants.SupportedDisplayModes.Contains(display))
                {
                    request.DisplayMode = display;
                }

                _logger.LogDebug("Unsupported display mode - ignored: " + display);
            }

            //////////////////////////////////////////////////////////
            // check max_age
            //////////////////////////////////////////////////////////
            var maxAge = request.Raw.Get(OidcConstants.AuthorizeRequest.MaxAge);
            if (maxAge.IsPresent())
            {
                if (int.TryParse(maxAge, out var seconds))
                {
                    if (seconds >= 0)
                    {
                        request.MaxAge = seconds;
                    }
                    else
                    {
                        LogError("Invalid max_age.", request);
                        return Invalid(request, subError: "invalid_max_age");
                    }
                }
                else
                {
                    LogError("Invalid max_age.", request);
                    return Invalid(request, subError: "invalid_max_age");
                }
            }

            //////////////////////////////////////////////////////////
            // check login_hint
            //////////////////////////////////////////////////////////
            var loginHint = request.Raw.Get(OidcConstants.AuthorizeRequest.LoginHint);
            if (loginHint.IsPresent())
            {
                if (loginHint.Length > _options.InputLengthRestrictions.LoginHint)
                {
                    LogError("Login hint too long", request);
                    return Invalid(request, subError: "invalid_login_hint");
                }

                request.LoginHint = loginHint;
            }

            //////////////////////////////////////////////////////////
            // check acr_values
            //////////////////////////////////////////////////////////
            var acrValues = request.Raw.Get(OidcConstants.AuthorizeRequest.AcrValues);
            if (acrValues.IsPresent())
            {
                if (acrValues.Length > _options.InputLengthRestrictions.AcrValues)
                {
                    LogError("Acr values too long", request);
                    return Invalid(request, subError: "invalid_acr_values");
                }

                request.AuthenticationContextReferenceClasses = acrValues.FromSpaceSeparatedString().Distinct().ToList();
            }

            //////////////////////////////////////////////////////////
            // check custom acr_values: idp
            //////////////////////////////////////////////////////////
            var idp = request.GetIdP();
            if (idp.IsPresent())
            {
                // if idp is present but client does not allow it, strip it from the request message
                if (request.Client.IdentityProviderRestrictions != null && request.Client.IdentityProviderRestrictions.Any())
                {
                    if (!request.Client.IdentityProviderRestrictions.Contains(idp))
                    {
                        _logger.LogWarning("idp requested ({idp}) is not in client restriction list.", idp);
                        request.RemoveIdP();
                    }
                }
            }

            //////////////////////////////////////////////////////////
            // check session cookie
            //////////////////////////////////////////////////////////
            if (_options.Endpoints.EnableCheckSessionEndpoint)
            {
                if (request.Subject.IsAuthenticated())
                {
                    var sessionId = await _userSession.GetSessionIdAsync();
                    if (sessionId.IsPresent())
                    {
                        request.SessionId = sessionId;
                    }
                    else
                    {
                        LogError("Check session endpoint enabled, but SessionId is missing", request);
                    }
                }
                else
                {
                    request.SessionId = ""; // empty string for anonymous users
                }
            }

            _logger.LogInformation($"Started Business Scoped flow validation. Client ID: {request.ClientId}. Nonce: {request.Nonce}. Subject ID: {request.Subject.GetSubjectId()}");

            var businessSelectRequest = new BusinessSelectRequest(request.ClientId, request.Nonce, request.Subject.GetSubjectId());

            var businessSelect = await _businessSelectMessageStore.ReadAsync(businessSelectRequest.Id);

            if (businessSelect?.Data != null)
            {
                request.BusinessID = businessSelect.Data.BusinessId;
                request.SubjectBusinessScopedID = businessSelect.Data.SubjectBusinessScopedId;
            }

            return Valid(request);
        }

        private AuthorizeRequestValidationResult Invalid(ValidatedAuthorizeRequest request, string error = OidcConstants.AuthorizeErrors.InvalidRequest, string subError = null)
        {
            return new AuthorizeRequestValidationResult(request, error, subError);
        }

        private AuthorizeRequestValidationResult Valid(ValidatedAuthorizeRequest request)
        {
            return new AuthorizeRequestValidationResult(request);
        }

        private void LogError(string message, ValidatedAuthorizeRequest request)
        {
            var requestDetails = new AuthorizeRequestValidationLog(request, _options.Logging.AuthorizeRequestSensitiveValuesFilter);
            _logger.LogError(message + "\n{@requestDetails}", requestDetails);
        }

        private void LogError(string message, string detail, ValidatedAuthorizeRequest request)
        {
            var requestDetails = new AuthorizeRequestValidationLog(request, _options.Logging.AuthorizeRequestSensitiveValuesFilter);
            _logger.LogError(message + ": {detail}\n{@requestDetails}", detail, requestDetails);
        }
    }
}
