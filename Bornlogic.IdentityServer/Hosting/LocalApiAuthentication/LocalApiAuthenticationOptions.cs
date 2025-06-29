using Microsoft.AspNetCore.Authentication;

namespace Bornlogic.IdentityServer.Hosting.LocalApiAuthentication
{
    /// <summary>
    /// Options for local API authentication
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions" />
    public class LocalApiAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Allows setting a specific required scope (optional)
        /// </summary>
        public string ExpectedScope { get; set; }

        /// <summary>
        /// Specifies whether the token should be saved in the authentication properties
        /// </summary>
        public bool SaveToken { get; set; } = true;

        /// <summary>
        /// Allows implementing events
        /// </summary>
        public new LocalApiAuthenticationEvents Events
        {
            get { return (LocalApiAuthenticationEvents)base.Events; }
            set { base.Events = value; }
        }
    }
}