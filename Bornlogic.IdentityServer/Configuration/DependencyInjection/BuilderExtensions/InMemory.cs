// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Services.Default;
using Bornlogic.IdentityServer.Services.InMemory;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Services;
using Bornlogic.IdentityServer.Storage.Stores;
using Bornlogic.IdentityServer.Stores.InMemory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bornlogic.IdentityServer.Configuration.DependencyInjection.BuilderExtensions
{
    /// <summary>
    /// Builder extension methods for registering in-memory services
    /// </summary>
    public static class IdentityServerBuilderExtensionsInMemory
    {
        /// <summary>
        /// Adds the in memory caching.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryCaching(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();
            builder.Services.TryAddTransient(typeof(ICache<>), typeof(DefaultCache<>));

            return builder;
        }

        /// <summary>
        /// Adds the in memory identity resources.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="identityResources">The identity resources.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryIdentityResources(this IIdentityServerBuilder builder, IEnumerable<IdentityResource> identityResources)
        {
            builder.Services.AddSingleton(identityResources);
            builder.AddResourceStore<InMemoryResourcesStore>();

            return builder;
        }

        /// <summary>
        /// Adds the in memory identity resources.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="section">The configuration section containing the configuration data.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryIdentityResources(this IIdentityServerBuilder builder, IConfigurationSection section)
        {
            var resources = new List<IdentityResource>();
            section.Bind(resources);

            return builder.AddInMemoryIdentityResources(resources);
        }

        /// <summary>
        /// Adds the in memory API resources.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="apiResources">The API resources.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryApiResources(this IIdentityServerBuilder builder, IEnumerable<ApiResource> apiResources)
        {
            builder.Services.AddSingleton(apiResources);
            builder.AddResourceStore<InMemoryResourcesStore>();

            return builder;
        }
        
        /// <summary>
        /// Adds the in memory API resources.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="section">The configuration section containing the configuration data.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryApiResources(this IIdentityServerBuilder builder, IConfigurationSection section)
        {
            var resources = new List<ApiResource>();
            section.Bind(resources);

            return builder.AddInMemoryApiResources(resources);
        }

        /// <summary>
        /// Adds the in memory API scopes.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="apiScopes">The API scopes.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryApiScopes(this IIdentityServerBuilder builder, IEnumerable<ApiScope> apiScopes)
        {
            builder.Services.AddSingleton(apiScopes);
            builder.AddResourceStore<InMemoryResourcesStore>();

            return builder;
        }

        /// <summary>
        /// Adds the in memory scopes.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="section">The configuration section containing the configuration data.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryApiScopes(this IIdentityServerBuilder builder, IConfigurationSection section)
        {
            var resources = new List<ApiScope>();
            section.Bind(resources);

            return builder.AddInMemoryApiScopes(resources);
        }

        /// <summary>
        /// Adds the in memory clients.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="clients">The clients.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryClients(this IIdentityServerBuilder builder, IEnumerable<Client> clients)
        {
            builder.Services.AddSingleton(clients);

            builder.AddClientStore<InMemoryClientStore>();

            var existingCors = builder.Services.Where(x => x.ServiceType == typeof(ICorsPolicyService)).LastOrDefault();
            if (existingCors != null && 
                existingCors.ImplementationType == typeof(DefaultCorsPolicyService) && 
                existingCors.Lifetime == ServiceLifetime.Transient)
            {
                // if our default is registered, then overwrite with the InMemoryCorsPolicyService
                // otherwise don't overwrite with the InMemoryCorsPolicyService, which uses the custom one registered by the host
                builder.Services.AddTransient<ICorsPolicyService, InMemoryCorsPolicyService>();
            }

            return builder;
        }


        /// <summary>
        /// Adds the in memory clients.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="section">The configuration section containing the configuration data.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryClients(this IIdentityServerBuilder builder, IConfigurationSection section)
        {
            var clients = new List<Client>();
            section.Bind(clients);

            return builder.AddInMemoryClients(clients);
        }


        /// <summary>
        /// Adds the in memory stores.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddInMemoryPersistedGrants(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddSingleton<IPersistedGrantStore, InMemoryPersistedGrantStore>();
            builder.Services.TryAddSingleton<IDeviceFlowStore, InMemoryDeviceFlowStore>();

            return builder;
        }
    }
}