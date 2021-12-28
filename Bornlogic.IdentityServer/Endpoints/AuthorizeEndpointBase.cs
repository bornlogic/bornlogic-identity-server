﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Security.Claims;
using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Endpoints.Results;
using Bornlogic.IdentityServer.Events;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Hosting;
using Bornlogic.IdentityServer.Logging.Models;
using Bornlogic.IdentityServer.Models.Messages;
using Bornlogic.IdentityServer.ResponseHandling;
using Bornlogic.IdentityServer.ResponseHandling.Models;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Validation;
using Bornlogic.IdentityServer.Validation.Models;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Endpoints
{
    internal abstract class AuthorizeEndpointBase : IEndpointHandler
    {
        private readonly IAuthorizeResponseGenerator _authorizeResponseGenerator;

        private readonly IEventService _events;
        private readonly IdentityServerOptions _options;

        private readonly IAuthorizeInteractionResponseGenerator _interactionGenerator;

        private readonly IAuthorizeRequestValidator _validator;

        protected AuthorizeEndpointBase(
            IEventService events,
            ILogger<AuthorizeEndpointBase> logger,
            IdentityServerOptions options,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeResponseGenerator authorizeResponseGenerator,
            IUserSession userSession)
        {
            _events = events;
            _options = options;
            Logger = logger;
            _validator = validator;
            _interactionGenerator = interactionGenerator;
            _authorizeResponseGenerator = authorizeResponseGenerator;
            UserSession = userSession;
        }

        protected ILogger Logger { get; private set; }

        protected IUserSession UserSession { get; private set; }

        public abstract Task<IEndpointResult> ProcessAsync(HttpContext context);

        internal async Task<IEndpointResult> ProcessAuthorizeRequestAsync(NameValueCollection parameters, ClaimsPrincipal user, ConsentResponse consent)
        {
            if (user != null)
            {
                Logger.LogDebug("User in authorize request: {subjectId}", user.GetSubjectId());
            }
            else
            {
                Logger.LogDebug("No user present in authorize request");
            }

            // validate request
            var result = await _validator.ValidateAsync(parameters, user);
            if (result.IsError)
            {
                return await CreateErrorResultAsync(
                    "Request validation failed",
                    result.ValidatedRequest,
                    result.Error,
                    result.ErrorDescription);
            }

            if (user.HasClaim("email_confirmed", "false"))
            {
                return await CreateErrorResultAsync(
                    "Request validation failed",
                    result.ValidatedRequest,
                    "Email is not verified",
                    "The email must be confirmed before staring the authorization flow");
            }

            var request = result.ValidatedRequest;

            LogRequest(request);

            // determine user interaction
            var interactionResult = await _interactionGenerator.ProcessInteractionAsync(request, consent);
            if (interactionResult.IsError)
            {
                return await CreateErrorResultAsync("Interaction generator error", request, interactionResult.Error, interactionResult.ErrorDescription, false);
            }
            if (interactionResult.IsLogin)
            {
                return new LoginPageResult(request);
            }
            if (interactionResult.IsConsent)
            {
                return new ConsentPageResult(request);
            }
            if (interactionResult.IsRedirect)
            {
                return new CustomRedirectResult(request, interactionResult.RedirectUrl);
            }

            var response = await _authorizeResponseGenerator.CreateResponseAsync(request);

            await RaiseResponseEventAsync(response);

            LogResponse(response);

            return new AuthorizeResult(response);
        }

        protected async Task<IEndpointResult> CreateErrorResultAsync(
            string logMessage,
            ValidatedAuthorizeRequest request = null,
            string error = OidcConstants.AuthorizeErrors.ServerError,
            string errorDescription = null,
            bool logError = true)
        {
            if (logError)
            {
                Logger.LogError(logMessage);
            }

            if (request != null)
            {
                var details = new AuthorizeRequestValidationLog(request, _options.Logging.AuthorizeRequestSensitiveValuesFilter);
                Logger.LogInformation("{@validationDetails}", details);
            }

            // TODO: should we raise a token failure event for all errors to the authorize endpoint?
            await RaiseFailureEventAsync(request, error, errorDescription);

            return new AuthorizeResult(new AuthorizeResponse
            {
                Request = request,
                Error = error,
                ErrorDescription = errorDescription,
                SessionState = request?.GenerateSessionStateValue()
            });
        }

        private void LogRequest(ValidatedAuthorizeRequest request)
        {
            var details = new AuthorizeRequestValidationLog(request, _options.Logging.AuthorizeRequestSensitiveValuesFilter);
            Logger.LogDebug(nameof(ValidatedAuthorizeRequest) + Environment.NewLine + "{@validationDetails}", details);
        }

        private void LogResponse(AuthorizeResponse response)
        {
            var details = new AuthorizeResponseLog(response);
            Logger.LogDebug("Authorize endpoint response" + Environment.NewLine + "{@details}", details);
        }

        private void LogTokens(AuthorizeResponse response)
        {
            var clientId = $"{response.Request.ClientId} ({response.Request.Client.ClientName ?? "no name set"})";
            var subjectId = response.Request.Subject.GetSubjectId();

            if (response.IdentityToken != null)
            {
                Logger.LogTrace("Identity token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.IdentityToken);
            }
            if (response.Code != null)
            {
                Logger.LogTrace("Code issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.Code);
            }
            if (response.AccessToken != null)
            {
                Logger.LogTrace("Access token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.AccessToken);
            }
        }

        private Task RaiseFailureEventAsync(ValidatedAuthorizeRequest request, string error, string errorDescription)
        {
            return _events.RaiseAsync(new TokenIssuedFailureEvent(request, error, errorDescription));
        }

        private Task RaiseResponseEventAsync(AuthorizeResponse response)
        {
            if (!response.IsError)
            {
                LogTokens(response);
                return _events.RaiseAsync(new TokenIssuedSuccessEvent(response));
            }

            return RaiseFailureEventAsync(response.Request, response.Error, response.ErrorDescription);
        }
    }
}