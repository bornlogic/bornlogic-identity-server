using System.Security.Claims;

namespace Bornlogic.IdentityServer.Storage.Services
{
    public interface IUserManagerService
    {
        Task UpsertClaim(Claim claim);
    }
}
