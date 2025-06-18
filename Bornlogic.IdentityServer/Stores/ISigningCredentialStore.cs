



using Microsoft.IdentityModel.Tokens;

namespace Bornlogic.IdentityServer.Stores
{
    /// <summary>
    /// Interface for a signing credential store
    /// </summary>
    public interface ISigningCredentialStore
    {
        /// <summary>
        /// Gets the signing credentials.
        /// </summary>
        /// <returns></returns>
        Task<SigningCredentials> GetSigningCredentialsAsync();
    }
}