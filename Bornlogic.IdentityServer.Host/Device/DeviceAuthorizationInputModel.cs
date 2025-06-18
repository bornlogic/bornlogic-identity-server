



using Bornlogic.IdentityServer.Host.Consent;

namespace Bornlogic.IdentityServer.Host.Device
{
    public class DeviceAuthorizationInputModel : ConsentInputModel
    {
        public string UserCode { get; set; }
    }
}