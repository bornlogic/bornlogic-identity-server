namespace Bornlogic.IdentityServer.Storage
{
    public class UserSavedConsent
    {
        public string UserID { get; set; }
        public string ClientID { get; set; }
        public IEnumerable<string> Scopes { get; set; }
    }
}
