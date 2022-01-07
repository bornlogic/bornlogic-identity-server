using Microsoft.AspNetCore.Identity;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    internal class UserLoginsRepository : IUserLoginsRepository
    {
        public async Task<IEnumerable<UserLoginInfo>> GetByID(string userID)
        {
            return new List<UserLoginInfo>();
        }

        public async Task<IdentityUserLogin<string>> GetByFilters(string userID, string provider, string providerKey)
        {
            return new IdentityUserLogin<string>();
        }

        public async Task<IdentityUserLogin<string>> GetByFilters(string provider, string providerKey)
        {
            return new IdentityUserLogin<string>();
        }

        public Task Insert(string userID, UserLoginInfo login)
        {
          return Task.CompletedTask;
        }

        public Task DeleteByFilters(string userID, string provider, string providerKey)
        {
            return Task.CompletedTask;
        }
    }
}
