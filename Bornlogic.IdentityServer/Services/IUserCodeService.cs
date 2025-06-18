



namespace Bornlogic.IdentityServer.Services
{
    /// <summary>
    /// Implements user code generation
    /// </summary>
    public interface IUserCodeService
    {
        /// <summary>
        /// Gets the user code generator.
        /// </summary>
        /// <param name="userCodeType">Type of user code.</param>
        /// <returns></returns>
        Task<IUserCodeGenerator> GetGenerator(string userCodeType);
    }
}