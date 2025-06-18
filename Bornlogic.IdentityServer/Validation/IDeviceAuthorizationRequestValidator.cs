using System.Collections.Specialized;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.Validation
{
    /// <summary>
    ///  Device authorization endpoint request validator.
    /// </summary>
    public interface IDeviceAuthorizationRequestValidator
    {
        /// <summary>
        ///  Validates authorize request parameters.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="clientValidationResult"></param>
        /// <returns></returns>
        Task<DeviceAuthorizationRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClientSecretValidationResult clientValidationResult);
    }
}