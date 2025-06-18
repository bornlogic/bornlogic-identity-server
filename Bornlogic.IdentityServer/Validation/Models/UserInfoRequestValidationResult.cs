using System.Security.Claims;

namespace Bornlogic.IdentityServer.Validation.Models
{
    /// <summary>
    /// Validation result for userinfo requests
    /// </summary>
    public class UserInfoRequestValidationResult : ValidationResult
    {
        /// <summary>
        /// Gets or sets the token validation result.
        /// </summary>
        /// <value>
        /// The token validation result.
        /// </value>
        public TokenValidationResult TokenValidationResult { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public ClaimsPrincipal Subject { get; set; }
    }
}