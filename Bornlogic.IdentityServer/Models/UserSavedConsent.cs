namespace Bornlogic.IdentityServer.Models
{
    public class UserSavedConsent
    {
        public string UserID { get; set; }
        public string ClientID { get; set; }
        public IEnumerable<string> Scopes { get; set; }
    }
}
