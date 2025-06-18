



using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Services
{
    /// <summary>
    /// The device flow throttling service.
    /// </summary>
    public interface IDeviceFlowThrottlingService
    {
        /// <summary>
        /// Decides if the requesting client and device code needs to slow down.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <param name="details">The device code details.</param>
        /// <returns></returns>
        Task<bool> ShouldSlowDown(string deviceCode, DeviceCode details);
    }
}