using Bornlogic.IdentityServer.Validation.Models;
using Microsoft.AspNetCore.Http;

namespace Bornlogic.IdentityServer.Validation
{
    /// <summary>
    /// Validator for handling client authentication
    /// </summary>
    public interface IClientSecretValidator
    {
        /// <summary>
        /// Tries to authenticate a client based on the incoming request
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task<ClientSecretValidationResult> ValidateAsync(HttpContext context);
    }
}