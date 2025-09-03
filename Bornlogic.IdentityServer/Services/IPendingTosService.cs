using Bornlogic.IdentityServer.Models.Messages;
using Bornlogic.IdentityServer.Storage.Models;
using System.Security.Claims;

namespace Bornlogic.IdentityServer.Services
{
    public interface IPendingTosService
    {
        Task<bool> HasPendingTos(ClaimsPrincipal subject, Client client, BusinessSelectResponse businessSelect);
    }
}
