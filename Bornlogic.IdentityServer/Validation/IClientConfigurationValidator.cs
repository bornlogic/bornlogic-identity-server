using Bornlogic.IdentityServer.Validation.Contexts;

namespace Bornlogic.IdentityServer.Validation
{
    /// <summary>
    /// Validator for handling client authentication
    /// </summary>
    public interface IClientConfigurationValidator
    {
        /// <summary>
        /// Determines whether the configuration of a client is valid.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task ValidateAsync(ClientConfigurationValidationContext context);
    }
}