using System.Security.Claims;

namespace Bornlogic.IdentityServer.Services
{
    public interface IUserClaimsEnricher
    {
        Task<Claim[]> GetAdditionalClaims(ClaimsPrincipal principal);
    }
}
