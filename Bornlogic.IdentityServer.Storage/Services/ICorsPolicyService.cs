using Microsoft.AspNetCore.Http;

namespace Bornlogic.IdentityServer.Storage.Services
{
    /// <summary>
    /// Service that determines if CORS is allowed.
    /// </summary>
    public interface ICorsPolicyService
    {
        /// <summary>
        /// Determines whether origin is allowed.
        /// </summary>
        /// <param name="httpContext">The http context</param>
        /// <returns></returns>
        Task<bool> IsOriginAllowedAsync(HttpContext httpContext);
    }
}
