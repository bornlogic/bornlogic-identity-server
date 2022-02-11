using System.Security.Claims;
using System.Security.Principal;
using Bornlogic.IdentityServer.Storage.Services;

namespace Bornlogic.IdentityServer.Services.Default
{
    public  class DefaultUserManagerService : IUserManagerService
    {
        public Task UpsertClaim(IPrincipal currentPrincipal, Claim claim)
        {
            return Task.CompletedTask;
        }

        public Task RemoveClaimByType(IPrincipal currentPrincipal, string type)
        {
            return Task.CompletedTask;
        }
    }
}
