using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    internal class ApiScopeRepository : IApiScopeRepository
    {
        public async Task<IEnumerable<ApiScope>> GetAll()
        {
            
            return new[]
            {
                new ApiScope
                {
                    Enabled = true,
                    Name = "graph_api_access",
                    DisplayName = "graph_api_access",
                    ShowInDiscoveryDocument = true
                },
                new ApiScope
                {
                    Enabled = true,
                    Name = "api1",
                    DisplayName = "api1",
                    ShowInDiscoveryDocument = true
                },
                new ApiScope
                {
                    Enabled = true,
                    Name = "api2",
                    DisplayName = "api2",
                    ShowInDiscoveryDocument = true
                }
            };
        }
    }
}
