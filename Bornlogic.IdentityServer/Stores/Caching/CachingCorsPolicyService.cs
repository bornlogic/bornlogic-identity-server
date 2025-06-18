



using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Storage.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Stores.Caching
{
    /// <summary>
    /// Caching decorator for ICorsPolicyService
    /// </summary>
    /// <seealso cref="ICorsPolicyService" />
    public class CachingCorsPolicyService<T> : ICorsPolicyService
        where T : ICorsPolicyService
    {
        /// <summary>
        /// Class to model entries in CORS origin cache.
        /// </summary>
        public class CorsCacheEntry
        {
            /// <summary>
            /// Ctor.
            /// </summary>
            public CorsCacheEntry(bool allowed)
            {
                Allowed = allowed;
            }

            /// <summary>
            /// Is origin allowed.
            /// </summary>
            public bool Allowed { get; }
        }

        private readonly IdentityServerOptions Options;
        private readonly ICache<CorsCacheEntry> CorsCache;
        private readonly ICorsPolicyService Inner;
        private readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingResourceStore{T}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="corsCache">The CORS origin cache.</param>
        /// <param name="logger">The logger.</param>
        public CachingCorsPolicyService(IdentityServerOptions options,
            T inner,
            ICache<CorsCacheEntry> corsCache,
            ILogger<CachingCorsPolicyService<T>> logger)
        {
            Options = options;
            Inner = inner;
            CorsCache = corsCache;
            Logger = logger;
        }

        /// <summary>
        /// Determines whether origin is allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public virtual async Task<bool> IsOriginAllowedAsync(HttpContext httpContext)
        {
            var entry = await CorsCache.GetAsync(httpContext.Request.GetCorsOrigin(),
                          Options.Caching.CorsExpiration,
                          async () => new CorsCacheEntry(await Inner.IsOriginAllowedAsync(httpContext)),
                          Logger);

            return entry.Allowed;
        }
    }
}
