using System.Security.Claims;
using Bornlogic.IdentityServer.Models.Messages;
using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Services.Default
{
    public class DefaultPendingTosService : IPendingTosService
    {
        public Task<bool> HasPendingTos(ClaimsPrincipal subject, Client client, BusinessSelectResponse businessSelect)
        {
            return Task.FromResult(false);
        }
    }
}
