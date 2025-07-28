using System.Security.Claims;

namespace Bornlogic.IdentityServer.Services.Default
{
    public class DefaultUserClaimsEnricher : IUserClaimsEnricher
    {
        public Task<Claim[]> GetAdditionalClaims(ClaimsPrincipal principal) => Task.FromResult(Array.Empty<Claim>());
    }
}
