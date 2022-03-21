using System.Security.Claims;
using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Services
{
    public interface IRefreshTokenIssuanceService
    {
        Task<bool> CanIssueRefreshToken(ClaimsPrincipal subject, Client client);
    }
}
