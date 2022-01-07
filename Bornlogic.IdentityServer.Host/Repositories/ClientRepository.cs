using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    internal static class ClientsMemoryRepository
    {
        public static IList<Client> Clients = new List<Client>
        {
            new Client
            {
                Enabled = true,
                ClientId = "mvc",
                ProtocolType = "oidc",
                ClientSecrets = new List<Secret>
                {
                    new Secret
                    {
                        Value = "K7gNU3sdo+OL0wNhqoVWhr3g6s1xYv72ol/pe/Unols=",
                        Type = "SharedSecret"
                    }
                },
                RequireClientSecret = true,
                RequireConsent = true,
                AllowRememberConsent = true,
                AllowedGrantTypes = new List<string>
                {
                    "authorization_code"
                },
                RequirePkce = true,
                RedirectUris = new List<string>
                {
                    "https://localhost:5002/signin-oidc"
                },
                PostLogoutRedirectUris = new List<string>
                {
                    "https://localhost:5002/signout-callback-oidc"
                },
                FrontChannelLogoutSessionRequired = true,
                BackChannelLogoutSessionRequired = true,
                AllowOfflineAccess = true,
                AllowedScopes = new List<string>
                {
                    "openid",
                    "profile",
                    "api1",
                    "api2"
                },
                IdentityTokenLifetime = 300,
                AccessTokenLifetime = 3000,
                AuthorizationCodeLifetime = 300,
                AbsoluteRefreshTokenLifetime = 2592000,
                SlidingRefreshTokenLifetime = 1296000,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                AccessTokenType = AccessTokenType.Reference,
                EnableLocalLogin = true,
                IncludeJwtId = true,
                ClientClaimsPrefix = "client_",
                DeviceCodeLifetime = 300
            }
        };
    }

    internal class ClientRepository : IClientRepository
    {
        public async Task<Client> GetByID(string id)
        {
            return ClientsMemoryRepository.Clients.FirstOrDefault(a => a.ClientId == id);
        }
    }
}
