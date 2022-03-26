using System.Security.Claims;
using Bornlogic.IdentityServer.Models;
using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Services
{
    public interface IRefreshTokenIssuanceService
    {
        Task<bool> CanIssueRefreshToken(ClaimsPrincipal subject, Client client);
        Task<RefreshTokenLifetimeResult> GetRefreshTokenLifetimeInSeconds(ClaimsPrincipal subject, Client client);
    }
}
