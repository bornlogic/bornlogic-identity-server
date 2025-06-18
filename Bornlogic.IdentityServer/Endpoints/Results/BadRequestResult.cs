



using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Hosting;
using Microsoft.AspNetCore.Http;

namespace Bornlogic.IdentityServer.Endpoints.Results
{
    internal class BadRequestResult : IEndpointResult
    {
        public string Error { get; set; }
        public string SubError { get; set; }

        public BadRequestResult(string error = null, string subError = null)
        {
            Error = error;
            SubError = subError;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.StatusCode = 400;
            context.Response.SetNoCache();

            if (Error.IsPresent())
            {
                var dto = new ResultDto
                {
                    error = Error,
                    sub_error = SubError
                };

                await context.Response.WriteJsonAsync(dto);
            }
        }

        internal class ResultDto
        {
            public string error { get; set; }
            public string sub_error { get; set; }
        }    
    }
}