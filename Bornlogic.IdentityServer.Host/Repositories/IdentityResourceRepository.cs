using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    internal class IdentityResourceRepository : IIdentityResourceRepository
    {
        public async Task<IEnumerable<IdentityResource>> GetAll()
        {
            return new[]
            {
                new IdentityResource
                {
                    Enabled = true,
                    Name = "openid",
                    DisplayName = "Your user identifier",
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<string>
                    {
                        "sub"
                    },
                    Required = true
                },
                new IdentityResource
                {
                    Enabled = true,
                    Name = "profile",
                    DisplayName = "User profile",
                    Description = "Your user profile information (first name, last name, etc.)",
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<string>
                    {
                        "sub"
                    },
                    Required = true
                },
                new IdentityResource
                {
                    Enabled = true,
                    Name = "email",
                    DisplayName = "Your email address",
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<string>
                    {
                        "sub"
                    },
                    Required = true
                }
            };
        }

        public async Task<IEnumerable<IdentityResource>> GetByScopeNames(IEnumerable<string> scopeNames)
        {
            var scopeNamesAsLit = scopeNames.ToList();

            return (await GetAll()).Where(a => scopeNamesAsLit.Contains(a.Name)).ToList();
        }
    }
}
