



using System.Text.Json.Serialization;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Hosting;
using Bornlogic.IdentityServer.ResponseHandling.Models;
using Microsoft.AspNetCore.Http;

namespace Bornlogic.IdentityServer.Endpoints.Results
{
    internal class TokenErrorResult : IEndpointResult
    {
        public TokenErrorResponse Response { get; }

        public TokenErrorResult(TokenErrorResponse error)
        {
            if (error.Error.IsMissing()) throw new ArgumentNullException(nameof(error.Error), "Error must be set");

            Response = error;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.StatusCode = 400;
            context.Response.SetNoCache();

            var dto = new ResultDto
            {
                error = Response.Error,
                sub_error = Response.SubError,
                
                custom = Response.Custom
            };

            await context.Response.WriteJsonAsync(dto);
        }

        internal class ResultDto
        {
            public string error { get; set; }
            public string sub_error { get; set; }

            [JsonExtensionData]
            public Dictionary<string, object> custom { get; set; }
        }    
    }
}
