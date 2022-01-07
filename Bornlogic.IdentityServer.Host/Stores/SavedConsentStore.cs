using Bornlogic.IdentityServer.Storage;
using Bornlogic.IdentityServer.Stores;

namespace Bornlogic.IdentityServer.Host.Stores
{
    public class SavedConsentStore : ISavedConsentStore
    {
        public async Task<UserSavedConsent> GetFromFilters(string userID, string clientID, IEnumerable<string> requiredScopes)
        {
            return default;
        }

        public async Task DeleteFromFilters(string userID, string clientID)
        {
        }

        public async Task Store(UserSavedConsent userSavedConsent)
        {
        }
    }
}
