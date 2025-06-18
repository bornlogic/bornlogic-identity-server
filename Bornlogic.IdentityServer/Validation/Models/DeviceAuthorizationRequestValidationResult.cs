namespace Bornlogic.IdentityServer.Validation.Models
{
    /// <summary>
    /// Validation result for device authorization requests
    /// </summary>
    public class DeviceAuthorizationRequestValidationResult : ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceAuthorizationRequestValidationResult"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public DeviceAuthorizationRequestValidationResult(ValidatedDeviceAuthorizationRequest request)
        {
            IsError = false;

            ValidatedRequest = request;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceAuthorizationRequestValidationResult"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="error">The error.</param>
        /// <param name="subError">The sub error.</param>
        public DeviceAuthorizationRequestValidationResult(ValidatedDeviceAuthorizationRequest request, string error, string subError = null)
        {
            IsError = true;

            Error = error;
            SubError = subError;
            ValidatedRequest = request;
        }

        /// <summary>
        /// Gets the validated request.
        /// </summary>
        /// <value>
        /// The validated request.
        /// </value>
        public ValidatedDeviceAuthorizationRequest ValidatedRequest { get; }
    }
}