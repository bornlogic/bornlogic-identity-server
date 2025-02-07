﻿using Bornlogic.IdentityServer.Host.Repositories;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;

namespace Bornlogic.IdentityServer.Host.Stores
{
    public class ClientStore : IClientStore
    {
        private readonly IClientRepository _clientStoreRepository;

        public ClientStore(IClientRepository clientStoreRepository)
        {
            _clientStoreRepository = clientStoreRepository;
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return _clientStoreRepository.GetByID(clientId);
        }

        public Task UpdateClient(Client client)
        {
            return _clientStoreRepository.Update(client);
        }

        public Task InsertClient(Client client)
        {
            return _clientStoreRepository.Insert(client);
        }
    }
}