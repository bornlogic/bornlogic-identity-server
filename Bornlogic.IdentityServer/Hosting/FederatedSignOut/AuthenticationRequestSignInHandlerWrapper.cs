using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Bornlogic.IdentityServer.Hosting.FederatedSignOut
{
    internal class AuthenticationRequestSignInHandlerWrapper : AuthenticationRequestSignOutHandlerWrapper, IAuthenticationSignInHandler
    {
        private readonly IAuthenticationSignInHandler _inner;

        public AuthenticationRequestSignInHandlerWrapper(IAuthenticationSignInHandler inner, IHttpContextAccessor httpContextAccessor)
            : base(inner, httpContextAccessor)
        {
            _inner = inner;
        }

        public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            return _inner.SignInAsync(user, properties);
        }
    }
}
