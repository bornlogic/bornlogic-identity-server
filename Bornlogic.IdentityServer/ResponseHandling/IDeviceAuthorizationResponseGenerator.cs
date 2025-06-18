



using Bornlogic.IdentityServer.ResponseHandling.Models;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.ResponseHandling
{
    /// <summary>
    /// Interface for the device authorization response generator
    /// </summary>
    public interface IDeviceAuthorizationResponseGenerator
    {
        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns></returns>
        Task<DeviceAuthorizationResponse> ProcessAsync(DeviceAuthorizationRequestValidationResult validationResult, string baseUrl);
    }
}