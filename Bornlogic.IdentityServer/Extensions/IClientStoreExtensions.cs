using System.Security.Claims;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;

namespace Bornlogic.IdentityServer.Extensions
{
    /// <summary>
    /// Extension for IClientStore
    /// </summary>
    public static class IClientStoreExtensions
    {
        /// <summary>
        /// Finds the enabled client by identifier.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public static async Task<Client> FindEnabledClientByIdAsync(this IClientStore store, string clientId, IClientUserRoleService clientUserRoleService, string userID)
        {
            var client = await store.FindClientByIdAsync(clientId);

            if (client != null)
            {
                if (client.Enabled)
                {
                    return client;
                }
                
                if (clientUserRoleService != null && !string.IsNullOrEmpty(userID))
                {
                    var userHasLoginByPassRoleInClient = await clientUserRoleService.UserHasLoginByPassRoleInClient(userID, client, null);

                    if (userHasLoginByPassRoleInClient)
                    {
                        return client;
                    }
                }
            }

            return null;
        }
    }
}