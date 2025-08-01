using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Storage.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Services.Default
{
    /// <summary>
    /// Default CORS policy service.
    /// </summary>
    public class DefaultCorsPolicyService : ICorsPolicyService
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCorsPolicyService"/> class.
        /// </summary>
        public DefaultCorsPolicyService(ILogger<DefaultCorsPolicyService> logger)
        {
            Logger = logger;
            AllowedOrigins = new HashSet<string>();
        }

        /// <summary>
        /// The list allowed origins that are allowed.
        /// </summary>
        /// <value>
        /// The allowed origins.
        /// </value>
        public ICollection<string> AllowedOrigins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all origins are allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if allow all; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAll { get; set; }

        /// <summary>
        /// Determines whether the origin allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public virtual Task<bool> IsOriginAllowedAsync(HttpContext httpContext)
        {
            var origin = httpContext.Request.GetCorsOrigin();

            if (!String.IsNullOrWhiteSpace(origin))
            {
                if (AllowAll)
                {
                    Logger.LogDebug("AllowAll true, so origin: {0} is allowed", origin);
                    return Task.FromResult(true);
                }

                if (AllowedOrigins != null)
                {
                    if (AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                    {
                        Logger.LogDebug("AllowedOrigins configured and origin {0} is allowed", origin);
                        return Task.FromResult(true);
                    }
                    else
                    {
                        Logger.LogDebug("AllowedOrigins configured and origin {0} is not allowed", origin);
                    }
                }

                Logger.LogDebug("Exiting; origin {0} is not allowed", origin);
            }

            return Task.FromResult(false);
        }
    }
}
