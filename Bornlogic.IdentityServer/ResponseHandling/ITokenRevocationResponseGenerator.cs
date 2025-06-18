



using Bornlogic.IdentityServer.ResponseHandling.Models;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.ResponseHandling
{
    /// <summary>
    /// Interface for the userinfo response generator
    /// </summary>
    public interface ITokenRevocationResponseGenerator
    {
        /// <summary>
        /// Creates the revocation endpoint response and processes the revocation request.
        /// </summary>
        /// <param name="validationResult">The userinfo request validation result.</param>
        /// <returns></returns>
        Task<TokenRevocationResponse> ProcessAsync(TokenRevocationRequestValidationResult validationResult);
    }
}