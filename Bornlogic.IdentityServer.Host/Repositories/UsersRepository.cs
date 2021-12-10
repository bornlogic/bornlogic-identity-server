using System.Security.Claims;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    internal static class UsersMemoryRepository
    {
        public static IList<ApplicationUser> Users = new List<ApplicationUser>();
    }

    internal class UsersRepository : IUserRepository
    {
        public Task Insert(ApplicationUser user)
        {
            UsersMemoryRepository.Users.Add(user);
            return Task.CompletedTask;
        }

        public Task Update(ApplicationUser user)
        {
            UsersMemoryRepository.Users = UsersMemoryRepository.Users.Where(a => a.Id != user.Id).ToList();
            UsersMemoryRepository.Users.Add(user);

            return Task.CompletedTask;
        }

        public Task UpdateClaims(string userID, IEnumerable<Claim> claims)
        {
            return Task.CompletedTask;
        }

        public Task DeleteByID(string id)
        {
            UsersMemoryRepository.Users = UsersMemoryRepository.Users.Where(a => a.Id != id).ToList();

            return Task.CompletedTask;
        }

        public async Task<ApplicationUser> GetByID(string id)
        {
            return UsersMemoryRepository.Users.FirstOrDefault(a => a.Id == id);
        }

        public async Task<IEnumerable<ApplicationUser>> GetByClaim(Claim claim)
        {
            var claimHash = $"{claim.Type}_{claim.Value}";

            return new List<ApplicationUser>();
        }

        public async Task<ApplicationUser> GetByUserName(string userName)
        {
            return UsersMemoryRepository.Users.FirstOrDefault(a => a.UserName == userName);
        }

        public async Task<ApplicationUser> GetByUserEmail(string email)
        {
            return UsersMemoryRepository.Users.FirstOrDefault(a => a.EmployeeId == email);
        }

        public async Task<ApplicationUser> GetByNormalizedUserName(string normalizedUserName)
        {
            return UsersMemoryRepository.Users.FirstOrDefault(a => a.NormalizedUserName == normalizedUserName);
        }

        public async Task<ApplicationUser> GetByNormalizedUserEmail(string normalizedUserEmail)
        {
            return UsersMemoryRepository.Users.FirstOrDefault(a => a.NormalizedEmail == normalizedUserEmail);
        }
    }
}
