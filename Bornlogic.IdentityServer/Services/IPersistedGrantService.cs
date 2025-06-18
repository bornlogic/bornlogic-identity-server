



using Bornlogic.IdentityServer.Models;

namespace Bornlogic.IdentityServer.Services
{
    /// <summary>
    /// Implements persisted grant logic
    /// </summary>
    public interface IPersistedGrantService
    {
        /// <summary>
        /// Gets all grants for a given subject ID.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <returns></returns>
        Task<IEnumerable<Grant>> GetAllGrantsAsync(string subjectId);

        /// <summary>
        /// Removes all grants for a given subject id, and optionally client id and session id combination.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier (optional).</param>
        /// <param name="sessionId">The sesion id (optional).</param>
        /// <returns></returns>
        Task RemoveAllGrantsAsync(string subjectId, string clientId = null, string sessionId = null);
    }
}