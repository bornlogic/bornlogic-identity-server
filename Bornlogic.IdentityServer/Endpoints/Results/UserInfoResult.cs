



using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Hosting;
using Microsoft.AspNetCore.Http;

namespace Bornlogic.IdentityServer.Endpoints.Results
{
    internal class UserInfoResult : IEndpointResult
    {
        public Dictionary<string, object> Claims;

        public UserInfoResult(Dictionary<string, object> claims)
        {
            Claims = claims;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.SetNoCache();
            await context.Response.WriteJsonAsync(Claims);
        }
    }
}