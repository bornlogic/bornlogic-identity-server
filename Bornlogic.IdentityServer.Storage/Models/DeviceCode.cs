using System.Security.Claims;

namespace Bornlogic.IdentityServer.Storage.Models
{
    /// <summary>
    /// Represents data needed for device flow.
    /// </summary>
    public class DeviceCode
    {
        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the lifetime.
        /// </summary>
        /// <value>
        /// The lifetime.
        /// </value>
        public int Lifetime { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets the description the user assigned to the device being authorized.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is open identifier.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is open identifier; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpenId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is authorized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is authorized; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// Gets or sets the requested scopes.
        /// </summary>
        /// <value>
        /// The authorized scopes.
        /// </value>
        public IEnumerable<string> RequestedScopes { get; set; }

        /// <summary>
        /// Gets or sets the authorized scopes.
        /// </summary>
        /// <value>
        /// The authorized scopes.
        /// </value>
        public IEnumerable<string> AuthorizedScopes { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public ClaimsPrincipal Subject { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; }
    }
}