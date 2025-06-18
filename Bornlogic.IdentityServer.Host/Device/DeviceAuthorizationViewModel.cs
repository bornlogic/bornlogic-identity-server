



using Bornlogic.IdentityServer.Host.Consent;

namespace Bornlogic.IdentityServer.Host.Device
{
    public class DeviceAuthorizationViewModel : ConsentViewModel
    {
        public string UserCode { get; set; }
        public bool ConfirmUserCode { get; set; }
    }
}