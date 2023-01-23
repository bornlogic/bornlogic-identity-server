// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Models.Contexts;
using Bornlogic.IdentityServer.Models.Messages;
using Bornlogic.IdentityServer.Models.Messages.Enums;
using Bornlogic.IdentityServer.ResponseHandling.Models;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Stores;
using Bornlogic.IdentityServer.Validation.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.ResponseHandling.Default
{
    /// <summary>
    /// Default logic for determining if user must login or consent when making requests to the authorization endpoint.
    /// </summary>
    /// <seealso cref="IAuthorizeInteractionResponseGenerator" />
    public class AuthorizeInteractionResponseGenerator : IAuthorizeInteractionResponseGenerator
    {
        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;
        
        /// <summary>
        /// The consent service.
        /// </summary>
        protected readonly IConsentService Consent;

        /// <summary>
        /// The profile service.
        /// </summary>
        protected readonly IProfileService Profile;

        /// <summary>
        /// The clock
        /// </summary>
        protected readonly ISystemClock Clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeInteractionResponseGenerator"/> class.
        /// </summary>
        /// <param name="clock">The clock.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="consent">The consent.</param>
        /// <param name="profile">The profile.</param>
        public AuthorizeInteractionResponseGenerator(
            ISystemClock clock,
            ILogger<AuthorizeInteractionResponseGenerator> logger,
            IConsentService consent,
            IProfileService profile)
        {
            Clock = clock;
            Logger = logger;
            Consent = consent;
            Profile = profile;
        }

        /// <summary>
        /// Processes the interaction logic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="consent">The consent.</param>
        /// <returns></returns>
        public virtual async Task<InteractionResponse> ProcessInteractionAsync(ValidatedAuthorizeRequest request, ConsentResponse consent = null, BusinessSelectResponse businessSelect = null)
        {
            Logger.LogTrace("ProcessInteractionAsync");

            if (consent != null && consent.Granted == false && consent.Error.HasValue && request.Subject.IsAuthenticated() == false)
            {
                // special case when anonymous user has issued an error prior to authenticating
                Logger.LogInformation("Error: User consent result: {error}", consent.Error);

                var error = consent.Error switch
                {
                    AuthorizationError.AccountSelectionRequired => OidcConstants.AuthorizeErrors.AccountSelectionRequired,
                    AuthorizationError.ConsentRequired => OidcConstants.AuthorizeErrors.ConsentRequired,
                    AuthorizationError.InteractionRequired => OidcConstants.AuthorizeErrors.InteractionRequired,
                    AuthorizationError.LoginRequired => OidcConstants.AuthorizeErrors.LoginRequired,
                    _ => OidcConstants.AuthorizeErrors.AccessDenied
                };

                return new InteractionResponse
                {
                    Error = error,
                    SubError = consent.SubError
                };
            }

            var result = await ProcessLoginAsync(request);

            if (!result.IsLogin && !result.IsError && !result.IsRedirect && businessSelect == null)
            {
                result = await ProcessConsentAsync(request, consent);
            }

            if (!result.IsLogin && !result.IsConsent && !result.IsError && !result.IsRedirect)
            {
                result = await ProcessBusinessSelectAsync(request, consent, businessSelect);
            }

            if ((result.IsLogin || result.IsConsent || result.IsRedirect) && request.PromptModes.Contains(OidcConstants.PromptModes.None))
            {
                // prompt=none means do not show the UI
                Logger.LogInformation("Changing response to LoginRequired: prompt=none was requested");
                result = new InteractionResponse
                {
                    Error = result.IsLogin ? OidcConstants.AuthorizeErrors.LoginRequired :
                                result.IsConsent ? OidcConstants.AuthorizeErrors.ConsentRequired :
                                    OidcConstants.AuthorizeErrors.InteractionRequired
                };
            }

            return result;
        }

        /// <summary>
        /// Processes the login logic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        protected internal virtual async Task<InteractionResponse> ProcessLoginAsync(ValidatedAuthorizeRequest request)
        {
            if (request.PromptModes.Contains(OidcConstants.PromptModes.Login) ||
                request.PromptModes.Contains(OidcConstants.PromptModes.SelectAccount))
            {
                Logger.LogInformation("Showing login: request contains prompt={0}", request.PromptModes.ToSpaceSeparatedString());

                // remove prompt so when we redirect back in from login page
                // we won't think we need to force a prompt again
                request.RemovePrompt();

                return new InteractionResponse { IsLogin = true, AdditionalQueryParameters = new Dictionary<string, string> { { "login_redirect_reason", "prompt_required" } } };
            }

            // unauthenticated user
            var isAuthenticated = request.Subject.IsAuthenticated();

            // user de-activated
            bool isActive = false;

            if (isAuthenticated)
            {
                var isActiveCtx = new IsActiveContext(request.Subject, request.Client, IdentityServerConstants.ProfileIsActiveCallers.AuthorizeEndpoint);
                await Profile.IsActiveAsync(isActiveCtx);

                isActive = isActiveCtx.IsActive;
            }

            if (!isAuthenticated || !isActive)
            {
                if (!isAuthenticated)
                {
                    Logger.LogInformation("Showing login: User is not authenticated");
                }
                else if (!isActive)
                {
                    Logger.LogInformation("Showing login: User is not active");
                }

                return new InteractionResponse { IsLogin = true, AdditionalQueryParameters = new Dictionary<string, string> { { "login_redirect_reason", "user_inactive" } } };
            }

            // check current idp
            var currentIdp = request.Subject.GetIdentityProvider();

            // check if idp login hint matches current provider
            var idp = request.GetIdP();
            if (idp.IsPresent())
            {
                if (idp != currentIdp)
                {
                    Logger.LogInformation("Showing login: Current IdP ({currentIdp}) is not the requested IdP ({idp})", currentIdp, idp);
                    return new InteractionResponse { IsLogin = true, AdditionalQueryParameters = new Dictionary<string, string> { { "login_redirect_reason", "idp_mismatch" } } };
                }
            }

            // check authentication freshness
            if (request.MaxAge.HasValue)
            {
                var authTime = request.Subject.GetAuthenticationTime();
                if (Clock.UtcNow > authTime.AddSeconds(request.MaxAge.Value))
                {
                    Logger.LogInformation("Showing login: Requested MaxAge exceeded.");

                    return new InteractionResponse { IsLogin = true, AdditionalQueryParameters = new Dictionary<string, string> { { "login_redirect_reason", "auth_max_age_reached" } } };
                }
            }

            // check local idp restrictions
            if (currentIdp == IdentityServerConstants.LocalIdentityProvider)
            {
                if (!request.Client.EnableLocalLogin)
                {
                    Logger.LogInformation("Showing login: User logged in locally, but client does not allow local logins");
                    return new InteractionResponse { IsLogin = true, AdditionalQueryParameters = new Dictionary<string, string> { { "login_redirect_reason", "client_local_logins_disabled" } } };
                }
            }
            // check external idp restrictions if user not using local idp
            else if (request.Client.IdentityProviderRestrictions != null &&
                request.Client.IdentityProviderRestrictions.Any() &&
                !request.Client.IdentityProviderRestrictions.Contains(currentIdp))
            {
                Logger.LogInformation("Showing login: User is logged in with idp: {idp}, but idp not in client restriction list.", currentIdp);
                return new InteractionResponse { IsLogin = true, AdditionalQueryParameters = new Dictionary<string, string> { { "login_redirect_reason", "idp_on_restriction_list" } } };
            }

            // check client's user SSO timeout
            if (request.Client.UserSsoLifetime.HasValue)
            {
                var authTimeEpoch = request.Subject.GetAuthenticationTimeEpoch();
                var nowEpoch = Clock.UtcNow.ToUnixTimeSeconds();

                var diff = nowEpoch - authTimeEpoch;
                if (diff > request.Client.UserSsoLifetime.Value)
                {
                    Logger.LogInformation("Showing login: User's auth session duration: {sessionDuration} exceeds client's user SSO lifetime: {userSsoLifetime}.", diff, request.Client.UserSsoLifetime);
                    return new InteractionResponse { IsLogin = true, AdditionalQueryParameters = new Dictionary<string, string> { { "login_redirect_reason", "user_sso_lifetime_reached" } } };
                }
            }

            if (request.Client.UserSsoExternalIdpLifetime.HasValue && (currentIdp != IdentityServerConstants.LocalIdentityProvider || request.Subject.GetAuthenticationMethod() == Constants.ExternalAuthenticationMethod))
            {
                var authTimeEpoch = request.Subject.GetAuthenticationTimeEpoch();
                var nowEpoch = Clock.UtcNow.ToUnixTimeSeconds();

                var diff = nowEpoch - authTimeEpoch;
                if (diff > request.Client.UserSsoExternalIdpLifetime.Value)
                {
                    Logger.LogInformation("Showing login: User's external auth session duration: {sessionDuration} exceeds client's user SSO lifetime: {userSsoLifetime}.", diff, request.Client.UserSsoExternalIdpLifetime);
                    return new InteractionResponse { IsLogin = true, AdditionalQueryParameters = new Dictionary<string, string> { { "login_redirect_reason", "user_sso_external_idp_lifetime_reached" } } };
                }
            }

            return new InteractionResponse();
        }

        /// <summary>
        /// Processes the consent logic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="consent">The consent.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Invalid PromptMode</exception>
        protected internal virtual async Task<InteractionResponse> ProcessConsentAsync(ValidatedAuthorizeRequest request, ConsentResponse consent = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request.PromptModes.Any() &&
                !request.PromptModes.Contains(OidcConstants.PromptModes.None) &&
                !request.PromptModes.Contains(OidcConstants.PromptModes.Consent))
            {
                Logger.LogError("Invalid prompt mode: {promptMode}", request.PromptModes.ToSpaceSeparatedString());
                throw new ArgumentException("Invalid PromptMode");
            }

            var consentRequired = await Consent.RequiresConsentAsync(request.Subject, request.Client, request.ValidatedResources.ParsedScopes);

            if (consentRequired && request.PromptModes.Contains(OidcConstants.PromptModes.None))
            {
                Logger.LogInformation("Error: prompt=none requested, but consent is required.");

                return new InteractionResponse
                {
                    Error = OidcConstants.AuthorizeErrors.ConsentRequired
                };
            }

            if (request.PromptModes.Contains(OidcConstants.PromptModes.Consent) || consentRequired)
            {
                var response = new InteractionResponse();

                // did user provide consent
                if (consent == null)
                {
                    // user was not yet shown conset screen
                    response.IsConsent = true;
                    Logger.LogInformation("Showing consent: User has not yet consented");
                }
                else
                {
                    request.WasConsentShown = true;
                    Logger.LogTrace("Consent was shown to user");

                    // user was shown consent -- did they say yes or no
                    if (consent.Granted == false)
                    {
                        // no need to show consent screen again
                        // build error to return to client
                        Logger.LogInformation("Error: User consent result: {error}", consent.Error);

                        var error = consent.Error switch
                        {
                            AuthorizationError.AccountSelectionRequired => OidcConstants.AuthorizeErrors.AccountSelectionRequired,
                            AuthorizationError.ConsentRequired => OidcConstants.AuthorizeErrors.ConsentRequired,
                            AuthorizationError.InteractionRequired => OidcConstants.AuthorizeErrors.InteractionRequired,
                            AuthorizationError.LoginRequired => OidcConstants.AuthorizeErrors.LoginRequired,
                            _ => OidcConstants.AuthorizeErrors.AccessDenied
                        };

                        response.Error = error;
                        response.SubError = consent.SubError;
                    }
                    else
                    {
                        // double check that required scopes are in the list of consented scopes
                        var requiredScopes = request.ValidatedResources.GetRequiredScopeValues();
                        var valid = requiredScopes.All(x => consent.ScopesValuesConsented.Contains(x));
                        if (valid == false)
                        {
                            response.Error = OidcConstants.AuthorizeErrors.AccessDenied;
                            Logger.LogInformation("Error: User denied consent to required scopes");
                        }
                        else
                        {
                            // they said yes, set scopes they chose
                            request.Description = consent.Description;
                            request.ValidatedResources = request.ValidatedResources.Filter(consent.ScopesValuesConsented);
                            Logger.LogInformation("User consented to scopes: {scopes}", consent.ScopesValuesConsented);

                            if (request.Client.AllowRememberConsent)
                            {
                                // remember consent
                                var parsedScopes = Enumerable.Empty<ParsedScopeValue>();
                                if (consent.RememberConsent)
                                {
                                    // remember what user actually selected
                                    parsedScopes = request.ValidatedResources.ParsedScopes;
                                    Logger.LogDebug("User indicated to remember consent for scopes: {scopes}", request.ValidatedResources.RawScopeValues);
                                }

                                await Consent.UpdateConsentAsync(request.Subject, request.Client, parsedScopes);
                            }
                        }
                    }
                }

                return response;
            }

            return new InteractionResponse();
        }

        protected internal virtual async Task<InteractionResponse> ProcessBusinessSelectAsync(ValidatedAuthorizeRequest request, ConsentResponse consent = null, BusinessSelectResponse businessSelect = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.WasConsentShown = consent != null;

            var requiresBusinessSelect = request.Client.RequiresBusinessSelection && string.IsNullOrEmpty(businessSelect?.BusinessId) && string.IsNullOrEmpty(businessSelect?.SubjectBusinessScopedId);

            return new InteractionResponse { IsBusinessSelect = requiresBusinessSelect };
        }
    }
}
