



using Bornlogic.IdentityServer.Models;

namespace Bornlogic.IdentityServer.Stores
{
    /// <summary>
    /// Interface for the validation key store
    /// </summary>
    public interface IValidationKeysStore
    {
        /// <summary>
        /// Gets all validation keys.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync();
    }
}