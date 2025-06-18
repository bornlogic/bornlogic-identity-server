



using Bornlogic.IdentityServer.Models;

namespace Bornlogic.IdentityServer.ResponseHandling
{
    /// <summary>
    /// Interface for discovery endpoint response generator
    /// </summary>
    public interface IDiscoveryResponseGenerator
    {
        /// <summary>
        /// Creates the discovery document.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="issuerUri">The issuer URI.</param>
        Task<Dictionary<string, object>> CreateDiscoveryDocumentAsync(string baseUrl, string issuerUri);

        /// <summary>
        /// Creates the JWK document.
        /// </summary>
        Task<IEnumerable<JsonWebKey>> CreateJwkDocumentAsync();
    }
}