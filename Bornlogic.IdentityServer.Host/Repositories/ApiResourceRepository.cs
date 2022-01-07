using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    internal class ApiResourceRepository : IApiResourceRepository
    {
        public async Task<IEnumerable<ApiResource>> GetAll()
        {
            return new[]
            {
                new ApiResource
                {
                    Name = "api1",
                    DisplayName = "api1",
                    ShowInDiscoveryDocument = true,
                    ApiSecrets = new List<Secret>
                    {
                        new Secret
                        {
                            Value =
                                "Bw+BDy4ASkdzbHDlq+A9BDkU/zPXD3JmmVAgZqC+UN6t2CL3mYP8YPc4ENooT2urXHqAtRKTE1Bk1PQ6Y1ZV5w==",
                            Type = "SharedSecret"
                        }
                    },
                    Scopes = new List<string>
                    {
                        "api1"
                    }
                }
            };
        }
    }
}
