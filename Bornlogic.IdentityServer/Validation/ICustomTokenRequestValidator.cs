



using Bornlogic.IdentityServer.Validation.Contexts;

namespace Bornlogic.IdentityServer.Validation
{
    /// <summary>
    /// Allows inserting custom validation logic into token requests
    /// </summary>
    public interface ICustomTokenRequestValidator
    {
        /// <summary>
        /// Custom validation logic for a token request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The validation result
        /// </returns>
        Task ValidateAsync(CustomTokenRequestValidationContext context);
    }
}