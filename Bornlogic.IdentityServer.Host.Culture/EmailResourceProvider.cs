using Bornlogic.IdentityServer.Culture.Contracts;

namespace Bornlogic.IdentityServer.Host.Culture
{
    public class EmailResourceProvider : IEmailResourceProvider
    {
        public string GetString(string key)
        {
            return key;
        }

        public string GetStringOrDefault(string key, string defaultValue)
        {
            return defaultValue;
        }
    }
}