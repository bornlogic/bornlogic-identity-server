using System.Net;
using Bornlogic.IdentityServer.Endpoints.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

#pragma warning disable 1591

namespace Bornlogic.IdentityServer.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetCorsOrigin(this HttpRequest request)
        {
            var origin = request.Headers["Origin"].FirstOrDefault();
            var thisOrigin = request.Scheme + "://" + request.Host;

            // see if the Origin is different than this server's origin. if so
            // that indicates a proper CORS request. some browsers send Origin
            // on POST requests.
            if (origin != null && origin != thisOrigin)
            {
                return origin;
            }

            return null;
        }

        public static bool TryGetClientID(this HttpRequest request, out string clientID)
        {
            if (!request.HasApplicationFormContentType())
            {
                clientID = null;
                return false;
            }

            var formValues = request.Form.AsNameValueCollection();

            clientID = formValues.Get("client_id");

            return !string.IsNullOrEmpty(clientID);
        }

        internal static bool HasApplicationFormContentType(this HttpRequest request)
        {
            if (request.ContentType is null) return false;
            
            if (MediaTypeHeaderValue.TryParse(request.ContentType, out var header))
            {
                // Content-Type: application/x-www-form-urlencoded; charset=utf-8
                return header.MediaType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}