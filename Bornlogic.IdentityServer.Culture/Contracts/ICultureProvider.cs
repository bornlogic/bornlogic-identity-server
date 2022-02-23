namespace Bornlogic.IdentityServer.Culture.Contracts
{
    public interface ICultureProvider
    {
        Task SetCulture(string language);
        string GetDefaultCulture();
        string GetCurrentCulture();
    }
}
