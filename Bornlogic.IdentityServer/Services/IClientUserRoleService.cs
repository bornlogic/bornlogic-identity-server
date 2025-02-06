using System.Security.Claims;
using Bornlogic.IdentityServer.Models;
using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Services
{
    public interface IClientUserRoleService
    {
        Task<bool> UserHasLoginByPassRoleInClient(string userID, Client client, string[] validRoles);
    }
}
