



using Bornlogic.IdentityServer.Validation.Contexts;

namespace Bornlogic.IdentityServer.Validation.Default
{
    /// <summary>
    /// Default custom request validator
    /// </summary>
    internal class DefaultCustomTokenRequestValidator : ICustomTokenRequestValidator
    {
        /// <summary>
        /// Custom validation logic for a token request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The validation result
        /// </returns>
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            return Task.CompletedTask;
        }
    }
}