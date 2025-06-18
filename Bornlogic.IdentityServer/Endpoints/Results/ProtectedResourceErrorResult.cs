using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Hosting;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Bornlogic.IdentityServer.Endpoints.Results
{
    internal class ProtectedResourceErrorResult : IEndpointResult
    {
        public string Error;
        public string SubError;

        public ProtectedResourceErrorResult(string error, string subError = null)
        {
            Error = error;
            SubError = subError;
        }

        public Task ExecuteAsync(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.SetNoCache();

            if (Constants.ProtectedResourceErrorStatusCodes.ContainsKey(Error))
            {
                context.Response.StatusCode = Constants.ProtectedResourceErrorStatusCodes[Error];
            }

            if (Error == OidcConstants.ProtectedResourceErrors.ExpiredToken)
            {
                Error = OidcConstants.ProtectedResourceErrors.InvalidToken;
                SubError = "expired_access_token";
            }

            var errorString = string.Format($"error=\"{Error}\"");
            if (SubError.IsMissing())
            {
                context.Response.Headers.Add(HeaderNames.WWWAuthenticate, new StringValues(new[] { "Bearer realm=\"IdentityServer\"", errorString }).ToString());
            }
            else
            {
                var subErrorString = string.Format($"sub_error=\"{SubError}\"");
                context.Response.Headers.Add(HeaderNames.WWWAuthenticate, new StringValues(new[] { "Bearer realm=\"IdentityServer\"", errorString, subErrorString }).ToString());
            }

            return Task.CompletedTask;
        }
    }
}
