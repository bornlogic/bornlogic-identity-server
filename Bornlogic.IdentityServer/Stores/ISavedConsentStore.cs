using Bornlogic.IdentityServer.Storage;

namespace Bornlogic.IdentityServer.Stores
{
    public interface ISavedConsentStore
    {
        Task<UserSavedConsent> GetFromFilters(string userID, string clientID, IEnumerable<string> requiredScopes);
        Task<IEnumerable<UserSavedConsent>> GetFromFilters(string userID, string clientID);
        Task DeleteFromFilters(string userID, string clientID);
        Task Store(UserSavedConsent userSavedConsent);
    }
}