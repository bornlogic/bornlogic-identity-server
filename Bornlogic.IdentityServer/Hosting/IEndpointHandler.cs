using Microsoft.AspNetCore.Http;

namespace Bornlogic.IdentityServer.Hosting
{
    /// <summary>
    /// Endpoint handler
    /// </summary>
    public interface IEndpointHandler
    {
        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        Task<IEndpointResult> ProcessAsync(HttpContext context);
    }
}