using System.Security.Claims;

namespace Bornlogic.IdentityServer.Services
{
    public interface IUserInfoClaimsEnricher
    {
        Task<Claim[]> GetAdditionalClaims(ClaimsPrincipal principal);
    }
}
