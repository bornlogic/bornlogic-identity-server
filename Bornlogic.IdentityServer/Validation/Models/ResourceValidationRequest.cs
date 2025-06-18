using System.Security.Claims;
using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Validation.Models
{
    /// <summary>
    /// Models the request to validate scopes and resource indicators for a client.
    /// </summary>
    public class ResourceValidationRequest
    {
        /// <summary>
        /// The client.
        /// </summary>
        public Client Client { get; set; }

        public ClaimsPrincipal Subject { get; set; }

        /// <summary>
        /// The requested scope values.
        /// </summary>
        public IEnumerable<string> Scopes { get; set; }

        public IEnumerable<string> RequiredRequestScopes { get; set; }
    }
}
