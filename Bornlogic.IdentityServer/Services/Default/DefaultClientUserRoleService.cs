using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Services.Default
{
    public class DefaultClientUserRoleService : IClientUserRoleService
    {
        public Task<bool> UserHasLoginByPassRoleInClient(string userID, Client client, string[] validRoles)
        {
            return Task.FromResult(false);
        }
    }
}