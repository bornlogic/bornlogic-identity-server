namespace Bornlogic.IdentityServer.Services
{
    /// <summary>
    /// Implements device flow user code generation
    /// </summary>
    public interface IUserCodeGenerator
    {
        /// <summary>
        /// Gets the type of the user code.
        /// </summary>
        /// <value>
        /// The type of the user code.
        /// </value>
        string UserCodeType { get; }

        /// <summary>
        /// Gets the retry limit.
        /// </summary>
        /// <value>
        /// The retry limit for getting a unique value.
        /// </value>
        int RetryLimit { get; }

        /// <summary>
        /// Generates the user code.
        /// </summary>
        /// <returns></returns>
        Task<string> GenerateAsync();
    }
}