using System.Security.Claims;
using Bornlogic.IdentityServer.Models;
using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Services.Default
{
    public class DefaultRefreshTokenIssuanceService : IRefreshTokenIssuanceService
    {
        public Task<bool> CanIssueRefreshToken(ClaimsPrincipal subject, Client client)
        {
            return Task.FromResult(true);
        }

        public Task<RefreshTokenLifetimeResult> GetRefreshTokenLifetimeInSeconds(ClaimsPrincipal subject, Client client)
        {
            return Task.FromResult(new RefreshTokenLifetimeResult
            {
                AbsoluteLifetime = client.AbsoluteRefreshTokenLifetime,
                SlidingLifetime = client.SlidingRefreshTokenLifetime
            });
        }
    }
}