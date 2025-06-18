



using Bornlogic.IdentityServer.Validation.Contexts;

namespace Bornlogic.IdentityServer.Validation.Default
{
    /// <summary>
    /// Default custom request validator
    /// </summary>
    internal class DefaultCustomAuthorizeRequestValidator : ICustomAuthorizeRequestValidator
    {
        /// <summary>
        /// Custom validation logic for the authorize request.
        /// </summary>
        /// <param name="context">The context.</param>
        public Task ValidateAsync(CustomAuthorizeRequestValidationContext context)
        {
            return Task.CompletedTask;
        }
    }
}