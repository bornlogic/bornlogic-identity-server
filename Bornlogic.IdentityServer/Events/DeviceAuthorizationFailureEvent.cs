



using Bornlogic.IdentityServer.Events.Infrastructure;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.Events
{
    /// <summary>
    /// Event for device authorization failure
    /// </summary>
    /// <seealso cref="Event" />
    public class DeviceAuthorizationFailureEvent : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceAuthorizationFailureEvent"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        public DeviceAuthorizationFailureEvent(DeviceAuthorizationRequestValidationResult result)
            : this()
        {
            if (result.ValidatedRequest != null)
            {
                ClientId = result.ValidatedRequest.Client?.ClientId;
                ClientName = result.ValidatedRequest.Client?.ClientName;
                Scopes = result.ValidatedRequest.RequestedScopes?.ToSpaceSeparatedString();
                
            }

            Endpoint = Constants.EndpointNames.DeviceAuthorization;
            Error = result.Error;
            SubError = result.SubError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceAuthorizationFailureEvent"/> class.
        /// </summary>
        public DeviceAuthorizationFailureEvent()
            : base(EventCategories.DeviceFlow,
                "Device Authorization Failure",
                EventTypes.Failure,
                EventIds.DeviceAuthorizationFailure)
        {
        }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        /// <value>
        /// The endpoint.
        /// </value>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public string Scopes { get; set; }
        
        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the sub error.
        /// </summary>
        /// <value>
        /// The sub error.
        /// </value>
        public string SubError { get; set; }
    }
}