



using Bornlogic.IdentityServer.Models;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.Validation
{
    /// <summary>
    /// Service for validating a received secret against a stored secret
    /// </summary>
    public interface ISecretValidator
    {
        /// <summary>
        /// Validates a secret
        /// </summary>
        /// <param name="secrets">The stored secrets.</param>
        /// <param name="parsedSecret">The received secret.</param>
        /// <returns>A validation result</returns>
        Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret);
    }
}