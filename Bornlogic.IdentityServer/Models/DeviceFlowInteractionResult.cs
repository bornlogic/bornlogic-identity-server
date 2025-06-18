


namespace Bornlogic.IdentityServer.Models
{
    /// <summary>
    /// Request object for device flow interaction
    /// </summary>
    public class DeviceFlowInteractionResult
    {
        /// <summary>
        /// Gets or sets the sub error.
        /// </summary>
        /// <value>
        /// The sub error.
        /// </value>
        public string SubError { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is error; otherwise, <c>false</c>.
        /// </value>
        public bool IsError { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is access denied.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is access denied; otherwise, <c>false</c>.
        /// </value>
        public bool IsAccessDenied { get; set; }

        /// <summary>
        /// Create failure result
        /// </summary>
        /// <param name="subError">The sub error.</param>
        /// <returns></returns>
        public static DeviceFlowInteractionResult Failure(string subError = null)
        {
            return new DeviceFlowInteractionResult
            {
                IsError = true,
                SubError = subError
            };
        }
    }
}