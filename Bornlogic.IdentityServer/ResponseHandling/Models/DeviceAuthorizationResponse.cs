#pragma warning disable 1591

namespace Bornlogic.IdentityServer.ResponseHandling.Models
{
    public class DeviceAuthorizationResponse
    {
        public string DeviceCode { get; set; }
        public string UserCode { get; set; }
        public string VerificationUri { get; set; }

        public string VerificationUriComplete { get; set; }
        public int DeviceCodeLifetime { get; set; }
        public int Interval { get; set; }
    }
}