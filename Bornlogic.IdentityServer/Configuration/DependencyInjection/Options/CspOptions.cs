



using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Configuration.DependencyInjection.Options
{
    /// <summary>
    /// Options for Content Security Policy
    /// </summary>
    public class CspOptions
    {
        /// <summary>
        /// Gets or sets the minimum CSP level.
        /// </summary>
        public CspLevel Level { get; set; } = CspLevel.Two;

        /// <summary>
        /// Gets or sets a value indicating whether the deprected X-Content-Security-Policy header should be added.
        /// </summary>
        public bool AddDeprecatedHeader { get; set; } = true;
    }
}