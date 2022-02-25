namespace Bornlogic.IdentityServer.Culture.Contracts
{
    public interface IResourceProvider
    {
        string GetString(string key);
        string GetStringOrDefault(string key, string defaultValue);
    }
}
