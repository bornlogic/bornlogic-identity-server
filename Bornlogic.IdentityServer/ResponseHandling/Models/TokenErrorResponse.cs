using IdentityModel;

namespace Bornlogic.IdentityServer.ResponseHandling.Models
{
    /// <summary>
    /// Models a token error response
    /// </summary>
    public class TokenErrorResponse
    {
        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public string Error { get; set; } = OidcConstants.TokenErrors.InvalidRequest;

        /// <summary>
        /// Gets or sets the sub error.
        /// </summary>
        /// <value>
        /// The sub error.
        /// </value>
        public string SubError { get; set; }

        /// <summary>
        /// Gets or sets the custom entries.
        /// </summary>
        /// <value>
        /// The custom.
        /// </value>
        public Dictionary<string, object> Custom { get; set; } = new Dictionary<string, object>();
    }
}