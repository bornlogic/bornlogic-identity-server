// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Models;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;
using Bornlogic.IdentityServer.Validation.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bornlogic.IdentityServer.Validation.Default
{
    /// <summary>
    /// Default implementation of IResourceValidator.
    /// </summary>
    public class DefaultResourceValidator : IResourceValidator
    {
        private readonly ILogger _logger;
        private readonly IClientUserRoleService _clientUserRoleService;
        private readonly IOptions<ClientRoleOptions> _clientRoleOptions;
        private readonly IScopeParser _scopeParser;
        private readonly IResourceStore _store;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResourceValidator"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="scopeParser"></param>
        /// <param name="logger">The logger.</param>
        public DefaultResourceValidator(IResourceStore store, IScopeParser scopeParser, ILogger<DefaultResourceValidator> logger, IClientUserRoleService clientUserRoleService, IOptions<ClientRoleOptions> clientRoleOptions)
        {
            _logger = logger;
            _clientUserRoleService = clientUserRoleService;
            _clientRoleOptions = clientRoleOptions;
            _scopeParser = scopeParser;
            _store = store;
        }

        /// <inheritdoc/>
        public virtual async Task<ResourceValidationResult> ValidateRequestedResourcesAsync(ResourceValidationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var parsedScopesResult = _scopeParser.ParseScopeValues(request.Scopes);

            var result = new ResourceValidationResult();

            if (!parsedScopesResult.Succeeded)
            {
                foreach (var invalidScope in parsedScopesResult.Errors)
                {
                    _logger.LogError("Invalid parsed scope {scope}, message: {error}", invalidScope.RawValue, invalidScope.Error);
                    result.InvalidScopes.Add(invalidScope.RawValue);
                }

                return result;
            }

            var parsedRequiredRequestScopesResult = _scopeParser.ParseScopeValues(request.RequiredRequestScopes);

            if (!parsedRequiredRequestScopesResult.Succeeded)
            {
                foreach (var invalidScope in parsedRequiredRequestScopesResult.Errors)
                {
                    _logger.LogError("Invalid parsed scope {scope}, message: {error}", invalidScope.RawValue, invalidScope.Error);
                    result.InvalidScopes.Add(invalidScope.RawValue);
                }

                return result;
            }

            var subjectIdOrDefault = request.Subject?.GetSubjectIdOrDefault();

            var userHasLoginByPassRoleInClient = !string.IsNullOrEmpty(subjectIdOrDefault) &&
                                                 await _clientUserRoleService.UserHasLoginByPassRoleInClient(
                                                     subjectIdOrDefault, request.Client,
                                                     _clientRoleOptions?.Value
                                                         ?.ValidUserRolesToBypassClientScopeValidation);

            var scopeNames = parsedScopesResult.ParsedScopes.Select(x => x.ParsedName).Distinct().ToArray();
            var resourcesFromStore = await _store.FindEnabledResourcesByScopeAsync(scopeNames);

            foreach (var scope in parsedScopesResult.ParsedScopes)
            {
                await ValidateScopeAsync(request.Client, resourcesFromStore, scope, result, request.RequiredRequestScopes.Any(a => a == scope.ParsedName), userHasLoginByPassRoleInClient);
            }

            var requiredRequestScopeNames = parsedRequiredRequestScopesResult.ParsedScopes.Select(x => x.ParsedName).Distinct().ToArray();
            var requiredRequestResourcesFromStore = await _store.FindEnabledResourcesByScopeAsync(requiredRequestScopeNames);

            foreach (var scope in parsedRequiredRequestScopesResult.ParsedScopes)
            {
                await ValidateRequestRequiredScopeAsync(request.Client, requiredRequestResourcesFromStore, scope, result, userHasLoginByPassRoleInClient);
            }

            if (result.InvalidScopes.Count > 0)
            {
                result.Resources.IdentityResources.Clear();
                result.Resources.ApiResources.Clear();
                result.Resources.ApiScopes.Clear();
                result.ParsedScopes.Clear();
            }

            return result;
        }

        /// <summary>
        /// Validates that the requested scopes is contained in the store, and the client is allowed to request it.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resourcesFromStore"></param>
        /// <param name="requestedScope"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task ValidateScopeAsync(
            Client client,
            Resources resourcesFromStore,
            ParsedScopeValue requestedScope,
            ResourceValidationResult result,
            bool forceRequired,
            bool userHasLoginByPassRoleInClient)
        {
            if (requestedScope.ParsedName == IdentityServerConstants.StandardScopes.OfflineAccess)
            {
                if (userHasLoginByPassRoleInClient || await IsClientAllowedOfflineAccessAsync(client))
                {
                    result.Resources.OfflineAccess = true;
                    result.ParsedScopes.Add(new ParsedScopeValue(IdentityServerConstants.StandardScopes.OfflineAccess));
                }
                else
                {
                    result.InvalidScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                }
            }
            else
            {
                var identity = resourcesFromStore.FindIdentityResourcesByScope(requestedScope.ParsedName);
                if (identity != null)
                {
                    if (userHasLoginByPassRoleInClient || await IsClientAllowedIdentityResourceAsync(client, identity))
                    {
                        result.ParsedScopes.Add(requestedScope);
                        result.Resources.IdentityResources.Add(identity);
                    }
                    else
                    {
                        result.InvalidScopes.Add(requestedScope.RawValue);
                    }
                }
                else
                {
                    var apiScope = resourcesFromStore.FindApiScope(requestedScope.ParsedName);
                    if (apiScope != null)
                    {
                        if (userHasLoginByPassRoleInClient || await IsClientAllowedApiScopeAsync(client, apiScope))
                        {
                            result.ParsedScopes.Add(requestedScope);

                            if (forceRequired)
                                apiScope.Required = true;

                            result.Resources.ApiScopes.Add(apiScope);

                            var apis = resourcesFromStore.FindApiResourcesByScope(apiScope.Name);
                            foreach (var api in apis)
                            {
                                result.Resources.ApiResources.Add(api);
                            }
                        }
                        else
                        {
                            result.InvalidScopes.Add(requestedScope.RawValue);
                        }
                    }
                    else
                    {
                        _logger.LogError("Scope {scope} not found in store.", requestedScope.ParsedName);
                        result.InvalidScopes.Add(requestedScope.RawValue);
                    }
                }
            }
        }

        protected virtual async Task ValidateRequestRequiredScopeAsync(Client client, Resources resourcesFromStore, ParsedScopeValue requestedScope, ResourceValidationResult result,
            bool userHasLoginByPassRoleInClient)
        {
            if (requestedScope.ParsedName == IdentityServerConstants.StandardScopes.OfflineAccess)
            {
                result.InvalidScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
            }
            else
            {
                var identity = resourcesFromStore.FindIdentityResourcesByScope(requestedScope.ParsedName);
                if (identity != null)
                {
                    if (!userHasLoginByPassRoleInClient && !(await IsClientAllowedIdentityResourceAsync(client, identity)))
                    {
                        result.InvalidScopes.Add(requestedScope.RawValue);
                    }
                }
                else
                {
                    var apiScope = resourcesFromStore.FindApiScope(requestedScope.ParsedName);
                    if (apiScope != null)
                    {
                        if (!userHasLoginByPassRoleInClient && !(await IsClientAllowedApiScopeAsync(client, apiScope)))
                        {
                            result.InvalidScopes.Add(requestedScope.RawValue);
                        }
                    }
                    else
                    {
                        _logger.LogError("Scope {scope} not found in store.", requestedScope.ParsedName);
                        result.InvalidScopes.Add(requestedScope.RawValue);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if client is allowed access to the identity scope.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        protected virtual Task<bool> IsClientAllowedIdentityResourceAsync(Client client, IdentityResource identity)
        {
            var allowed = client.AllowedScopes.Any(a => a.Name == identity.Name);
            if (!allowed)
            {
                _logger.LogError("Client {client} is not allowed access to scope {scope}.", client.ClientId, identity.Name);
            }
            return Task.FromResult(allowed);
        }

        /// <summary>
        /// Determines if client is allowed access to the API scope.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="apiScope"></param>
        /// <returns></returns>
        protected virtual Task<bool> IsClientAllowedApiScopeAsync(Client client, ApiScope apiScope)
        {
            var allowed = client.AllowedScopes.Any(a => a.Name == apiScope.Name);
            if (!allowed)
            {
                _logger.LogError("Client {client} is not allowed access to scope {scope}.", client.ClientId, apiScope.Name);
            }
            return Task.FromResult(allowed);
        }

        /// <summary>
        /// Validates if the client is allowed offline_access.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected virtual Task<bool> IsClientAllowedOfflineAccessAsync(Client client)
        {
            var allowed = client.AllowOfflineAccess;
            if (!allowed)
            {
                _logger.LogError("Client {client} is not allowed access to scope offline_access (via AllowOfflineAccess setting).", client.ClientId);
            }
            return Task.FromResult(allowed);
        }
    }
}