using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.ResponseHandling
{
    /// <summary>
    /// Interface for the userinfo response generator
    /// </summary>
    public interface IUserInfoResponseGenerator
    {
        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <param name="validationResult">The userinfo request validation result.</param>
        /// <returns></returns>
        Task<Dictionary<string, object>> ProcessAsync(UserInfoRequestValidationResult validationResult);
    }
}