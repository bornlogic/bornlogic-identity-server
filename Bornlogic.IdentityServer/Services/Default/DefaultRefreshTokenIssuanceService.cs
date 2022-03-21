using System.Security.Claims;
using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Services.Default
{
    public class DefaultRefreshTokenIssuanceService : IRefreshTokenIssuanceService
    {
        public Task<bool> CanIssueRefreshToken(ClaimsPrincipal subject, Client client)
        {
            return Task.FromResult(true);
        }
    }
}