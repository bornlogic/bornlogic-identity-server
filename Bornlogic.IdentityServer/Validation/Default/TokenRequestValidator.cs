using System.Collections.Specialized;
using System.Text;
using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Events;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Logging.Models;
using Bornlogic.IdentityServer.Models.Contexts;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;
using Bornlogic.IdentityServer.Validation.Contexts;
using Bornlogic.IdentityServer.Validation.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Validation.Default
{
    internal class TokenRequestValidator : ITokenRequestValidator
    {
        private readonly IdentityServerOptions _options;
        private readonly IAuthorizationCodeStore _authorizationCodeStore;
        private readonly ExtensionGrantValidator _extensionGrantValidator;
        private readonly ICustomTokenRequestValidator _customRequestValidator;
        private readonly IResourceValidator _resourceValidator;
        private readonly IResourceStore _resourceStore;
        private readonly ITokenValidator _tokenValidator;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IEventService _events;
        private readonly IResourceOwnerPasswordValidator _resourceOwnerValidator;
        private readonly IProfileService _profile;
        private readonly IDeviceCodeValidator _deviceCodeValidator;
        private readonly ISystemClock _clock;
        private readonly ILogger _logger;

        private ValidatedTokenRequest _validatedRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRequestValidator" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="authorizationCodeStore">The authorization code store.</param>
        /// <param name="resourceOwnerValidator">The resource owner validator.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="deviceCodeValidator">The device code validator.</param>
        /// <param name="extensionGrantValidator">The extension grant validator.</param>
        /// <param name="customRequestValidator">The custom request validator.</param>
        /// <param name="resourceValidator">The resource validator.</param>
        /// <param name="resourceStore">The resource store.</param>
        /// <param name="tokenValidator">The token validator.</param>
        /// <param name="refreshTokenService"></param>
        /// <param name="events">The events.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="logger">The logger.</param>
        public TokenRequestValidator(IdentityServerOptions options, 
            IAuthorizationCodeStore authorizationCodeStore, 
            IResourceOwnerPasswordValidator resourceOwnerValidator, 
            IProfileService profile, 
            IDeviceCodeValidator deviceCodeValidator, 
            ExtensionGrantValidator extensionGrantValidator, 
            ICustomTokenRequestValidator customRequestValidator,
            IResourceValidator resourceValidator,
            IResourceStore resourceStore,
            ITokenValidator tokenValidator, 
            IRefreshTokenService refreshTokenService,
            IEventService events, 
            ISystemClock clock, 
            ILogger<TokenRequestValidator> logger)
        {
            _logger = logger;
            _options = options;
            _clock = clock;
            _authorizationCodeStore = authorizationCodeStore;
            _resourceOwnerValidator = resourceOwnerValidator;
            _profile = profile;
            _deviceCodeValidator = deviceCodeValidator;
            _extensionGrantValidator = extensionGrantValidator;
            _customRequestValidator = customRequestValidator;
            _resourceValidator = resourceValidator;
            _resourceStore = resourceStore;
            _tokenValidator = tokenValidator;
            _refreshTokenService = refreshTokenService;
            _events = events;
        }

        /// <summary>
        /// Validates the request.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="clientValidationResult">The client validation result.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// parameters
        /// or
        /// client
        /// </exception>
        public async Task<TokenRequestValidationResult> ValidateRequestAsync(NameValueCollection parameters, ClientSecretValidationResult clientValidationResult)
        {
            _logger.LogDebug("Start token request validation");

            _validatedRequest = new ValidatedTokenRequest
            {
                Raw = parameters ?? throw new ArgumentNullException(nameof(parameters)),
                Options = _options
            };

            if (clientValidationResult == null) throw new ArgumentNullException(nameof(clientValidationResult));

            _validatedRequest.SetClient(clientValidationResult.Client, clientValidationResult.Secret, clientValidationResult.Confirmation);

            /////////////////////////////////////////////
            // check client protocol type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.ProtocolType != IdentityServerConstants.ProtocolTypes.OpenIdConnect)
            {
                LogError("Invalid protocol type for client",
                    new
                    {
                        clientId = _validatedRequest.Client.ClientId,
                        expectedProtocolType = IdentityServerConstants.ProtocolTypes.OpenIdConnect,
                        actualProtocolType = _validatedRequest.Client.ProtocolType
                    });

                return Invalid(OidcConstants.TokenErrors.InvalidClient, "Invalid protocol type for client");
            }

            /////////////////////////////////////////////
            // check grant type
            /////////////////////////////////////////////
            var grantType = parameters.Get(OidcConstants.TokenRequest.GrantType);
            if (grantType.IsMissing())
            {
                LogError("Grant type is missing");
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType, "Grant type is missing");
            }

            if (grantType.Length > _options.InputLengthRestrictions.GrantType)
            {
                LogError("Grant type is too long");
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType, "Grant type is too long");
            }

            _validatedRequest.GrantType = grantType;

            switch (grantType)
            {
                case OidcConstants.GrantTypes.AuthorizationCode:
                    return await RunValidationAsync(ValidateAuthorizationCodeRequestAsync, parameters);
                case OidcConstants.GrantTypes.ClientCredentials:
                    return await RunValidationAsync(ValidateClientCredentialsRequestAsync, parameters);
                case OidcConstants.GrantTypes.Password:
                    return await RunValidationAsync(ValidateResourceOwnerCredentialRequestAsync, parameters);
                case OidcConstants.GrantTypes.RefreshToken:
                    return await RunValidationAsync(ValidateRefreshTokenRequestAsync, parameters);
                case OidcConstants.GrantTypes.DeviceCode:
                    return await RunValidationAsync(ValidateDeviceCodeRequestAsync, parameters);
                default:
                    return await RunValidationAsync(ValidateExtensionGrantRequestAsync, parameters);
            }
        }

        private async Task<TokenRequestValidationResult> RunValidationAsync(Func<NameValueCollection, Task<TokenRequestValidationResult>> validationFunc, NameValueCollection parameters)
        {
            // run standard validation
            var result = await validationFunc(parameters);
            if (result.IsError)
            {
                return result;
            }

            // run custom validation
            _logger.LogTrace("Calling into custom request validator: {type}", _customRequestValidator.GetType().FullName);

            var customValidationContext = new CustomTokenRequestValidationContext { Result = result };
            await _customRequestValidator.ValidateAsync(customValidationContext);

            if (customValidationContext.Result.IsError)
            {
                if (customValidationContext.Result.Error.IsPresent())
                {
                    LogError("Custom token request validator", new { error = customValidationContext.Result.Error });
                }
                else
                {
                    LogError("Custom token request validator error");
                }

                return customValidationContext.Result;
            }

            LogSuccess();
            return customValidationContext.Result;
        }

        private async Task<TokenRequestValidationResult> ValidateAuthorizationCodeRequestAsync(NameValueCollection parameters)
        {
            _logger.LogDebug("Start validation of authorization code token request");

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (!_validatedRequest.Client.AllowedGrantTypes.ToList().Contains(GrantType.AuthorizationCode) &&
                !_validatedRequest.Client.AllowedGrantTypes.ToList().Contains(GrantType.Hybrid))
            {
                LogError("Client not authorized for code flow");
                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient, "Client not authorized for code flow");
            }

            /////////////////////////////////////////////
            // validate authorization code
            /////////////////////////////////////////////
            var code = parameters.Get(OidcConstants.TokenRequest.Code);
            if (code.IsMissing())
            {
                LogError("Authorization code is missing");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Authorization code is missing");
            }

            if (code.Length > _options.InputLengthRestrictions.AuthorizationCode)
            {
                LogError("Authorization code is too long");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Authorization code is too long");
            }

            _validatedRequest.AuthorizationCodeHandle = code;

            var authZcode = await _authorizationCodeStore.GetAuthorizationCodeAsync(code);
            if (authZcode == null)
            {
                LogError("Invalid authorization code", new { code });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Invalid authorization code");
            }
            
            /////////////////////////////////////////////
            // validate client binding
            /////////////////////////////////////////////
            if (authZcode.ClientId != _validatedRequest.Client.ClientId)
            {
                LogError("Client is trying to use a code from a different client", new { clientId = _validatedRequest.Client.ClientId, codeClient = authZcode.ClientId });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Client is trying to use a code from a different client");
            }

            // remove code from store
            // todo: set to consumed in the future?
            await _authorizationCodeStore.RemoveAuthorizationCodeAsync(code);

            if (authZcode.CreationTime.HasExceeded(authZcode.Lifetime, _clock.UtcNow.UtcDateTime))
            {
                LogError("Authorization code expired", new { code });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Authorization code expired");
            }

            /////////////////////////////////////////////
            // populate session id
            /////////////////////////////////////////////
            if (authZcode.SessionId.IsPresent())
            {
                _validatedRequest.SessionId = authZcode.SessionId;
            }

            /////////////////////////////////////////////
            // validate code expiration
            /////////////////////////////////////////////
            if (authZcode.CreationTime.HasExceeded(_validatedRequest.Client.AuthorizationCodeLifetime, _clock.UtcNow.UtcDateTime))
            {
                LogError("Authorization code is expired");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Authorization code is expired");
            }

            _validatedRequest.AuthorizationCode = authZcode;
            _validatedRequest.Subject = authZcode.Subject;

            /////////////////////////////////////////////
            // validate redirect_uri
            /////////////////////////////////////////////
            var redirectUri = parameters.Get(OidcConstants.TokenRequest.RedirectUri);
            if (redirectUri.IsMissing())
            {
                LogError("Redirect URI is missing");
                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient, "Redirect URI is missing");
            }

            if (redirectUri.Equals(_validatedRequest.AuthorizationCode.RedirectUri, StringComparison.Ordinal) == false)
            {
                LogError("Invalid redirect_uri", new { redirectUri, expectedRedirectUri = _validatedRequest.AuthorizationCode.RedirectUri });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Invalid redirect_uri");
            }

            /////////////////////////////////////////////
            // validate scopes are present
            /////////////////////////////////////////////
            if (_validatedRequest.AuthorizationCode.RequestedScopes == null ||
                !_validatedRequest.AuthorizationCode.RequestedScopes.Any())
            {
                LogError("Authorization code has no associated scopes");
                return Invalid(OidcConstants.TokenErrors.InvalidRequest, "Authorization code has no associated scopes");
            }

            /////////////////////////////////////////////
            // validate PKCE parameters
            /////////////////////////////////////////////
            var codeVerifier = parameters.Get(OidcConstants.TokenRequest.CodeVerifier);
            if (_validatedRequest.Client.RequirePkce || _validatedRequest.AuthorizationCode.CodeChallenge.IsPresent())
            {
                _logger.LogDebug("Client required a proof key for code exchange. Starting PKCE validation");

                var proofKeyResult = ValidateAuthorizationCodeWithProofKeyParameters(codeVerifier, _validatedRequest.AuthorizationCode);
                if (proofKeyResult.IsError)
                {
                    return proofKeyResult;
                }

                _validatedRequest.CodeVerifier = codeVerifier;
            }
            else
            {
                if (codeVerifier.IsPresent())
                {
                    LogError("Unexpected code_verifier: {codeVerifier}. This happens when the client is trying to use PKCE, but it is not enabled. Set RequirePkce to true.", codeVerifier);
                    return Invalid(OidcConstants.TokenErrors.InvalidGrant, "PKCE not enabled for client");
                }
            }

            /////////////////////////////////////////////
            // make sure user is enabled
            /////////////////////////////////////////////
            var isActiveCtx = new IsActiveContext(_validatedRequest.AuthorizationCode.Subject, _validatedRequest.Client, IdentityServerConstants.ProfileIsActiveCallers.AuthorizationCodeValidation);
            await _profile.IsActiveAsync(isActiveCtx);

            if (isActiveCtx.IsActive == false)
            {
                LogError("User has been disabled", new { subjectId = _validatedRequest.AuthorizationCode.Subject.GetSubjectId() });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "User has been disabled");
            }

            _logger.LogDebug("Validation of authorization code token request success");

            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateClientCredentialsRequestAsync(NameValueCollection parameters)
        {
            _logger.LogDebug("Start client credentials token request validation");

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (!_validatedRequest.Client.AllowedGrantTypes.ToList().Contains(GrantType.ClientCredentials))
            {
                LogError("Client not authorized for client credentials flow, check the AllowedGrantTypes setting", new { clientId = _validatedRequest.Client.ClientId });
                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient, "Client not authorized for client credentials flow, check the AllowedGrantTypes setting");
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (!await ValidateRequestedScopesAsync(parameters, ignoreImplicitIdentityScopes: true, ignoreImplicitOfflineAccess: true))
            {
                return Invalid(OidcConstants.TokenErrors.InvalidScope);
            }

            if (_validatedRequest.ValidatedResources.Resources.IdentityResources.Any())
            {
                LogError("Client cannot request OpenID scopes in client credentials flow", new { clientId = _validatedRequest.Client.ClientId });
                return Invalid(OidcConstants.TokenErrors.InvalidScope, "Client cannot request OpenID scopes in client credentials flow");
            }

            if (_validatedRequest.ValidatedResources.Resources.OfflineAccess)
            {
                LogError("Client cannot request a refresh token in client credentials flow", new { clientId = _validatedRequest.Client.ClientId });
                return Invalid(OidcConstants.TokenErrors.InvalidScope, "Client cannot request a refresh token in client credentials flow");
            }

            _logger.LogDebug("{clientId} credentials token request validation success", _validatedRequest.Client.ClientId);
            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateResourceOwnerCredentialRequestAsync(NameValueCollection parameters)
        {
            _logger.LogDebug("Start resource owner password token request validation");

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (!_validatedRequest.Client.AllowedGrantTypes.Contains(GrantType.ResourceOwnerPassword))
            {
                LogError("Client not authorized for resource owner flow, check the AllowedGrantTypes setting", new { client_id = _validatedRequest.Client.ClientId });
                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient, "Client not authorized for resource owner flow, check the AllowedGrantTypes setting");
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (!(await ValidateRequestedScopesAsync(parameters)))
            {
                return Invalid(OidcConstants.TokenErrors.InvalidScope);
            }

            /////////////////////////////////////////////
            // check resource owner credentials
            /////////////////////////////////////////////
            var userName = parameters.Get(OidcConstants.TokenRequest.UserName);
            var password = parameters.Get(OidcConstants.TokenRequest.Password);

            if (userName.IsMissing())
            {
                LogError("Username is missing");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Username is missing");
            }

            if (password.IsMissing())
            {
                password = "";
            }

            if (userName.Length > _options.InputLengthRestrictions.UserName ||
                password.Length > _options.InputLengthRestrictions.Password)
            {
                LogError("Username or password too long");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Username or password too long");
            }

            _validatedRequest.UserName = userName;


            /////////////////////////////////////////////
            // authenticate user
            /////////////////////////////////////////////
            var resourceOwnerContext = new ResourceOwnerPasswordValidationContext
            {
                UserName = userName,
                Password = password,
                Request = _validatedRequest
            };
            await _resourceOwnerValidator.ValidateAsync(resourceOwnerContext);

            if (resourceOwnerContext.Result.IsError)
            {
                // protect against bad validator implementations
                resourceOwnerContext.Result.Error ??= OidcConstants.TokenErrors.InvalidGrant;

                if (resourceOwnerContext.Result.Error == OidcConstants.TokenErrors.UnsupportedGrantType)
                {
                    LogError("Resource owner password credential grant type not supported");
                    await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, "password grant type not supported", resourceOwnerContext.Request.Client.ClientId);

                    return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType, "Resource owner password credential grant type not supported", customResponse: resourceOwnerContext.Result.CustomResponse);
                }

                var subError = "invalid_username_or_password";

                if (resourceOwnerContext.Result.SubError.IsPresent())
                {
                    subError = resourceOwnerContext.Result.SubError;
                }

                LogInformation("User authentication failed: ", subError ?? resourceOwnerContext.Result.Error);
                await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, subError, resourceOwnerContext.Request.Client.ClientId);

                return Invalid(resourceOwnerContext.Result.Error, subError, resourceOwnerContext.Result.CustomResponse);
            }

            if (resourceOwnerContext.Result.Subject == null)
            {
                var error = "User authentication failed: no principal returned";
                LogError(error);
                await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, error, resourceOwnerContext.Request.Client.ClientId);

                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "User authentication failed: no principal returned");
            }

            /////////////////////////////////////////////
            // make sure user is enabled
            /////////////////////////////////////////////
            var isActiveCtx = new IsActiveContext(resourceOwnerContext.Result.Subject, _validatedRequest.Client, IdentityServerConstants.ProfileIsActiveCallers.ResourceOwnerValidation);
            await _profile.IsActiveAsync(isActiveCtx);

            if (isActiveCtx.IsActive == false)
            {
                LogError("User has been disabled", new { subjectId = resourceOwnerContext.Result.Subject.GetSubjectId() });
                await RaiseFailedResourceOwnerAuthenticationEventAsync(userName, "user is inactive", resourceOwnerContext.Request.Client.ClientId);

                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "User has been disabled");
            }

            _validatedRequest.UserName = userName;
            _validatedRequest.Subject = resourceOwnerContext.Result.Subject;

            await RaiseSuccessfulResourceOwnerAuthenticationEventAsync(userName, resourceOwnerContext.Result.Subject.GetSubjectId(), resourceOwnerContext.Request.Client.ClientId);
            _logger.LogDebug("Resource owner password token request validation success.");
            return Valid(resourceOwnerContext.Result.CustomResponse);
        }

        private async Task<TokenRequestValidationResult> ValidateRefreshTokenRequestAsync(NameValueCollection parameters)
        {
            _logger.LogDebug("Start validation of refresh token request");

            var refreshTokenHandle = parameters.Get(OidcConstants.TokenRequest.RefreshToken);
            if (refreshTokenHandle.IsMissing())
            {
                LogError("Refresh token is missing");
                return Invalid(OidcConstants.TokenErrors.InvalidRequest, "Refresh token is missing");
            }

            if (refreshTokenHandle.Length > _options.InputLengthRestrictions.RefreshToken)
            {
                LogError("Refresh token too long");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Refresh token too long");
            }

            var result = await _refreshTokenService.ValidateRefreshTokenAsync(refreshTokenHandle, _validatedRequest.Client);

            if (result.IsError)
            {
                LogWarning("Refresh token validation failed. aborting");
                
                var subError = "Refresh token validation failed. It has expired or never existed.";

                if (_validatedRequest?.Client != null)
                {
                    subError =
                        $"{subError} For client '{_validatedRequest.Client.ClientId}', local IDP refresh tokens have a lifetime of {(_validatedRequest.Client.AbsoluteRefreshTokenLifetime / 3600)} hours, and external IDP refresh tokens have a lifetime of {(_validatedRequest.Client.AbsoluteSsoExternalIdpRefreshTokenLifetime / 3600)} hours.";
                }

                return Invalid(OidcConstants.TokenErrors.InvalidGrant, subError);
            }

            _validatedRequest.RefreshToken = result.RefreshToken;
            _validatedRequest.RefreshTokenHandle = refreshTokenHandle;
            _validatedRequest.Subject = result.RefreshToken.Subject;

            _logger.LogDebug("Validation of refresh token request success");
            // todo: more logging - similar to TokenValidator before
            
            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateDeviceCodeRequestAsync(NameValueCollection parameters)
        {
            _logger.LogDebug("Start validation of device code request");

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (!_validatedRequest.Client.AllowedGrantTypes.ToList().Contains(GrantType.DeviceFlow))
            {
                LogError("Client not authorized for device flow");
                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient, "Client not authorized for device flow");
            }

            /////////////////////////////////////////////
            // validate device code parameter
            /////////////////////////////////////////////
            var deviceCode = parameters.Get(OidcConstants.TokenRequest.DeviceCode);
            if (deviceCode.IsMissing())
            {
                LogError("Device code is missing");
                return Invalid(OidcConstants.TokenErrors.InvalidRequest, "Device code is missing");
            }

            if (deviceCode.Length > _options.InputLengthRestrictions.DeviceCode)
            {
                LogError("Device code too long");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Device code too long");
            }

            /////////////////////////////////////////////
            // validate device code
            /////////////////////////////////////////////
            var deviceCodeContext = new DeviceCodeValidationContext { DeviceCode = deviceCode, Request = _validatedRequest };
            await _deviceCodeValidator.ValidateAsync(deviceCodeContext);

            if (deviceCodeContext.Result.IsError) return deviceCodeContext.Result;

            _logger.LogDebug("Validation of authorization code token request success");

            return Valid();
        }

        private async Task<TokenRequestValidationResult> ValidateExtensionGrantRequestAsync(NameValueCollection parameters)
        {
            _logger.LogDebug("Start validation of custom grant token request");

            /////////////////////////////////////////////
            // check if client is allowed to use grant type
            /////////////////////////////////////////////
            if (!_validatedRequest.Client.AllowedGrantTypes.Contains(_validatedRequest.GrantType))
            {
                LogError("Client does not have the custom grant type in the allowed list, therefore requested grant is not allowed", new { clientId = _validatedRequest.Client.ClientId });
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }

            /////////////////////////////////////////////
            // check if a validator is registered for the grant type
            /////////////////////////////////////////////
            if (!_extensionGrantValidator.GetAvailableGrantTypes().Contains(_validatedRequest.GrantType, StringComparer.Ordinal))
            {
                LogError("No validator is registered for the grant type", new { grantType = _validatedRequest.GrantType });
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (!await ValidateRequestedScopesAsync(parameters))
            {
                return Invalid(OidcConstants.TokenErrors.InvalidScope);
            }

            /////////////////////////////////////////////
            // validate custom grant type
            /////////////////////////////////////////////
            var result = await _extensionGrantValidator.ValidateAsync(_validatedRequest);

            if (result == null)
            {
                LogError("Invalid extension grant");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Invalid extension grant");
            }

            if (result.IsError)
            {
                if (result.Error.IsPresent())
                {
                    LogError("Invalid extension grant", new { error = result.Error });
                    return Invalid(result.Error, result.SubError, result.CustomResponse);
                }
                else
                {
                    LogError("Invalid extension grant");
                    return Invalid(OidcConstants.TokenErrors.InvalidGrant, customResponse: result.CustomResponse);
                }
            }

            if (result.Subject != null)
            {
                /////////////////////////////////////////////
                // make sure user is enabled
                /////////////////////////////////////////////
                var isActiveCtx = new IsActiveContext(
                    result.Subject,
                    _validatedRequest.Client,
                    IdentityServerConstants.ProfileIsActiveCallers.ExtensionGrantValidation);

                await _profile.IsActiveAsync(isActiveCtx);

                if (isActiveCtx.IsActive == false)
                {
                    // todo: raise event?

                    LogError("User has been disabled", new { subjectId = result.Subject.GetSubjectId() });
                    return Invalid(OidcConstants.TokenErrors.InvalidGrant);
                }

                _validatedRequest.Subject = result.Subject;
            }

            _logger.LogDebug("Validation of extension grant token request success");
            return Valid(result.CustomResponse);
        }

        // todo: do we want to rework the semantics of these ignore params?
        // also seems like other workflows other than CC clients can omit scopes?
        private async Task<bool> ValidateRequestedScopesAsync(NameValueCollection parameters, bool ignoreImplicitIdentityScopes = false, bool ignoreImplicitOfflineAccess = false)
        {
            var scopes = parameters.Get(OidcConstants.TokenRequest.Scope);
            if (scopes.IsMissing())
            {
                _logger.LogTrace("Client provided no scopes - checking allowed scopes list");

                if (!_validatedRequest.Client.AllowedScopes.IsNullOrEmpty())
                {
                    // this finds all the scopes the client is allowed to access
                    var clientAllowedScopes = new List<string>();
                    if (!ignoreImplicitIdentityScopes)
                    {
                        var resources = await _resourceStore.FindResourcesByScopeAsync(_validatedRequest.Client.AllowedScopes.Select(a => a.Name));
                        clientAllowedScopes.AddRange(resources.ToScopeNames().Where(x => _validatedRequest.Client.AllowedScopes.Any(a => a.Name == x)));
                    }
                    else
                    {
                        var apiScopes = await _resourceStore.FindApiScopesByNameAsync(_validatedRequest.Client.AllowedScopes.Select(a => a.Name));
                        clientAllowedScopes.AddRange(apiScopes.Select(x => x.Name));
                    }

                    if (!ignoreImplicitOfflineAccess)
                    {
                        if (_validatedRequest.Client.AllowOfflineAccess)
                        {
                            clientAllowedScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                        }
                    }

                    scopes = clientAllowedScopes.Distinct().ToSpaceSeparatedString();
                    _logger.LogTrace("Defaulting to: {scopes}", scopes);
                }
                else
                {
                    LogError("No allowed scopes configured for client", new { clientId = _validatedRequest.Client.ClientId });
                    return false;
                }
            }

            if (scopes.Length > _options.InputLengthRestrictions.Scope)
            {
                LogError("Scope parameter exceeds max allowed length");
                return false;
            }

            var requestedScopes = scopes.ParseScopesString();

            if (requestedScopes == null)
            {
                LogError("No scopes found in request");
                return false;
            }

            var resourceValidationResult = await _resourceValidator.ValidateRequestedResourcesAsync(new ResourceValidationRequest { 
                Client = _validatedRequest.Client,
                Scopes = requestedScopes,
                Subject = _validatedRequest.Subject
            });

            if (!resourceValidationResult.Succeeded)
            {
                if (resourceValidationResult.InvalidScopes.Any())
                {
                    LogError("Invalid scopes requested");
                }
                else
                {
                    LogError("Invalid scopes for client requested");
                }

                return false;
            }

            _validatedRequest.RequestedScopes = requestedScopes;
            _validatedRequest.ValidatedResources = resourceValidationResult;
            
            return true;
        }

        private TokenRequestValidationResult ValidateAuthorizationCodeWithProofKeyParameters(string codeVerifier, AuthorizationCode authZcode)
        {
            if (authZcode.CodeChallenge.IsMissing() || authZcode.CodeChallengeMethod.IsMissing())
            {
                LogError("Client is missing code challenge or code challenge method", new { clientId = _validatedRequest.Client.ClientId });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Client is missing code challenge or code challenge method");
            }

            if (codeVerifier.IsMissing())
            {
                LogError("Missing code_verifier");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Missing code_verifier");
            }

            if (codeVerifier.Length < _options.InputLengthRestrictions.CodeVerifierMinLength ||
                codeVerifier.Length > _options.InputLengthRestrictions.CodeVerifierMaxLength)
            {
                LogError("code_verifier is too short or too long");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "code_verifier is too short or too long");
            }

            if (Constants.SupportedCodeChallengeMethods.Contains(authZcode.CodeChallengeMethod) == false)
            {
                LogError("Unsupported code challenge method", new { codeChallengeMethod = authZcode.CodeChallengeMethod });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Unsupported code challenge method");
            }

            if (ValidateCodeVerifierAgainstCodeChallenge(codeVerifier, authZcode.CodeChallenge, authZcode.CodeChallengeMethod) == false)
            {
                LogError("Transformed code verifier does not match code challenge");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant, "Transformed code verifier does not match code challenge");
            }

            return Valid();
        }

        private bool ValidateCodeVerifierAgainstCodeChallenge(string codeVerifier, string codeChallenge, string codeChallengeMethod)
        {
            if (codeChallengeMethod == OidcConstants.CodeChallengeMethods.Plain)
            {
                return TimeConstantComparer.IsEqual(codeVerifier.Sha256(), codeChallenge);
            }

            var codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);
            var hashedBytes = codeVerifierBytes.Sha256();
            var transformedCodeVerifier = Base64Url.Encode(hashedBytes);

            return TimeConstantComparer.IsEqual(transformedCodeVerifier.Sha256(), codeChallenge);
        }

        private TokenRequestValidationResult Valid(Dictionary<string, object> customResponse = null)
        {
            return new TokenRequestValidationResult(_validatedRequest, customResponse);
        }

        private TokenRequestValidationResult Invalid(string error, string subError = null, Dictionary<string, object> customResponse = null)
        {
            return new TokenRequestValidationResult(_validatedRequest, error, subError, customResponse);
        }

        private void LogError(string message = null, object values = null)
        {
            LogWithRequestDetails(LogLevel.Error, message, values);
        }

        private void LogWarning(string message = null, object values = null)
        {
            LogWithRequestDetails(LogLevel.Warning, message, values);
        }

        private void LogInformation(string message = null, object values = null)
        {
            LogWithRequestDetails(LogLevel.Information, message, values);
        }

        private void LogWithRequestDetails(LogLevel logLevel, string message = null, object values = null)
        {
            var details = new TokenRequestValidationLog(_validatedRequest, _options.Logging.TokenRequestSensitiveValuesFilter);

            if (message.IsPresent())
            {
                try
                {
                    if (values == null)
                    {
                        _logger.Log(logLevel, message + ", {@details}", details);
                    }
                    else
                    {
                        _logger.Log(logLevel, message + "{@values}, details: {@details}", values, details);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError("Error logging {exception}, request details: {@details}", ex.Message, details);
                }
            }
            else
            {
                _logger.Log(logLevel, "{@details}", details);
            }
        }

        private void LogSuccess()
        {
            LogWithRequestDetails(LogLevel.Information, "Token request validation success");
        }

        private Task RaiseSuccessfulResourceOwnerAuthenticationEventAsync(string userName, string subjectId, string clientId)
        {
            return _events.RaiseAsync(new UserLoginSuccessEvent(userName, subjectId, null, interactive: false, clientId));
        }

        private Task RaiseFailedResourceOwnerAuthenticationEventAsync(string userName, string error, string clientId)
        {
            return _events.RaiseAsync(new UserLoginFailureEvent(userName, error, interactive: false, clientId: clientId));
        }
    }
}
