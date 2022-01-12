using System.Security.Claims;
using System.Security.Principal;

namespace Bornlogic.IdentityServer.Storage.Services
{
    public interface IUserManagerService
    {
        Task UpsertClaim(IPrincipal currentPrincipal, Claim claim);
    }
}
