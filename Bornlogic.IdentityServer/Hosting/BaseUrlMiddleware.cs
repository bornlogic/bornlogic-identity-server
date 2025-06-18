using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;

#pragma warning disable 1591

namespace Bornlogic.IdentityServer.Hosting
{
    public class BaseUrlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IdentityServerOptions _options;

        public BaseUrlMiddleware(RequestDelegate next, IdentityServerOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            
            context.SetIdentityServerBasePath(request.PathBase.Value.RemoveTrailingSlash());

            await _next(context);
        }
    }
}