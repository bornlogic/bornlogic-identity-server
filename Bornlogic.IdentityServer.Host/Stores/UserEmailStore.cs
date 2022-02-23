using Bornlogic.IdentityServer.Storage.Stores;

namespace Bornlogic.IdentityServer.Host.Stores
{
    public class UserEmailStore : IUserEmailStore
    {
        public async Task<bool> UserEmailIsConfirmedAsync(string subjectId)
        {
            return true;
        }
    }
}
