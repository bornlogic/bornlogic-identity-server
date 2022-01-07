using Microsoft.AspNetCore.Identity;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    internal class UserTokensRepository : IUserTokensRepository
    {
        public async Task<IdentityUserToken<string>> GetByFilters(string userID, string name, string provider)
        {
            return new IdentityUserToken<string>();
        }

        public Task Insert(IdentityUserToken<string> token)
        {
            return Task.CompletedTask;
        }

        public Task DeleteByFilters(string userID, string name, string provider)
        {
            return Task.CompletedTask;
        }
    }
}
