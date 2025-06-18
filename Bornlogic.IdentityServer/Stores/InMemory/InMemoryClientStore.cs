



using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;

namespace Bornlogic.IdentityServer.Stores.InMemory
{
    /// <summary>
    /// In-memory client store
    /// </summary>
    public class InMemoryClientStore : IClientStore
    {
        private IList<Client> _clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryClientStore"/> class.
        /// </summary>
        /// <param name="clients">The clients.</param>
        public InMemoryClientStore(IList<Client> clients)
        {
            if (clients.HasDuplicates(m => m.ClientId))
            {
                throw new ArgumentException("Clients must not contain duplicate ids");
            }
            _clients = clients;
        }

        /// <summary>
        /// Finds a client by id
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>
        /// The client
        /// </returns>
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var query =
                from client in _clients
                where client.ClientId == clientId
                select client;
            
            return Task.FromResult(query.SingleOrDefault());
        }

        public Task UpdateClient(Client client)
        {
            _clients = _clients.Where(a => a.ClientId != client.ClientId).ToList();
            _clients.Add(client);

            return Task.CompletedTask;
        }

        public Task InsertClient(Client client)
        {
            _clients.Add(client);
            return Task.CompletedTask;
        }
    }
}