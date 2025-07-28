using System.Security.Claims;

namespace Bornlogic.IdentityServer.Services.Default
{
    public class DefaultUserInfoClaimsEnricher : IUserInfoClaimsEnricher
    {
        public Task<Claim[]> GetAdditionalClaims(ClaimsPrincipal principal) => Task.FromResult(Array.Empty<Claim>());
    }
}
