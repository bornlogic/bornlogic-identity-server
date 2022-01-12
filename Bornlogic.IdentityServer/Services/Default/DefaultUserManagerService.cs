using System.Security.Claims;
using Bornlogic.IdentityServer.Storage.Services;

namespace Bornlogic.IdentityServer.Services.Default
{
    public  class DefaultUserManagerService : IUserManagerService
    {
        public Task UpsertClaim(Claim claim)
        {
            return Task.CompletedTask;
        }
    }
}
