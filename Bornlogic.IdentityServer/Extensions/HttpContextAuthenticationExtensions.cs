using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bornlogic.IdentityServer.Extensions
{
    /// <summary>
    /// Extension methods for signin/out using the IdentityServer authentication scheme.
    /// </summary>
    public static class AuthenticationManagerExtensions
    {
        /// <summary>
        /// Signs the user in.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <param name="user">The IdentityServer user.</param>
        /// <returns></returns>
        public static async Task SignInAsync(this HttpContext context, IdentityServerUser user)
        {
            await context.SignInAsync(await context.GetCookieAuthenticationSchemeAsync(), user.CreatePrincipal());
        }

        /// <summary>
        /// Signs the user in.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <param name="user">The IdentityServer user.</param>
        /// <param name="properties">The authentication properties.</param>
        /// <returns></returns>
        public static async Task SignInAsync(this HttpContext context, IdentityServerUser user, AuthenticationProperties properties)
        {
            await context.SignInAsync(await context.GetCookieAuthenticationSchemeAsync(), user.CreatePrincipal(), properties);
        }

        internal static ISystemClock GetClock(this HttpContext context)
        {
            return context.RequestServices.GetRequiredService<ISystemClock>();
        }

        internal static async Task<string> GetCookieAuthenticationSchemeAsync(this HttpContext context)
        {
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            if (options.Authentication.CookieAuthenticationScheme != null)
            {
                return options.Authentication.CookieAuthenticationScheme;
            }

            var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var scheme = await schemes.GetDefaultAuthenticateSchemeAsync();
            if (scheme == null)
            {
                throw new InvalidOperationException("No DefaultAuthenticateScheme found or no CookieAuthenticationScheme configured on IdentityServerOptions.");
            }

            return scheme.Name;
        }
    }
}