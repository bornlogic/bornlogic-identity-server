namespace Bornlogic.IdentityServer.Services
{
    /// <summary>
    /// Interface for the handle generation service
    /// </summary>
    public interface IHandleGenerationService
    {
        /// <summary>
        /// Generates a handle.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        Task<string> GenerateAsync(int length = 32);
    }
}