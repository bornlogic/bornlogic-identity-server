



using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Storage.Stores
{
    /// <summary>
    /// Retrieval of client configuration
    /// </summary>
    public interface IClientStore
    {
        /// <summary>
        /// Finds a client by id
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>The client</returns>
        Task<Client> FindClientByIdAsync(string clientId);

        Task UpdateClient(Client client);

        Task InsertClient(Client client);
    }
}