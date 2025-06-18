



using Bornlogic.IdentityServer.ResponseHandling.Models;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.ResponseHandling
{
    /// <summary>
    /// Interface the token response generator
    /// </summary>
    public interface ITokenResponseGenerator
    {
        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <returns></returns>
        Task<TokenResponse> ProcessAsync(TokenRequestValidationResult validationResult);
    }
}