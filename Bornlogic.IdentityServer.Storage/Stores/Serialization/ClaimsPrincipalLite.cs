#pragma warning disable 1591

namespace Bornlogic.IdentityServer.Storage.Stores.Serialization
{
    public class ClaimsPrincipalLite
    {
        public string AuthenticationType { get; set; }
        public ClaimLite[] Claims { get; set; }
    }
}
