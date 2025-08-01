using Bornlogic.IdentityServer.Storage.Models;

#pragma warning disable 1591

namespace Bornlogic.IdentityServer.Models
{
    public class GrantTypes
    {
        public static ICollection<string> Implicit =>
            new[] { GrantType.Implicit };

        public static ICollection<string> ImplicitAndClientCredentials =>
            new[]  { GrantType.Implicit, GrantType.ClientCredentials };

        public static ICollection<string> Code =>
            new[] { GrantType.AuthorizationCode };

        public static ICollection<string> CodeAndClientCredentials =>
            new[] { GrantType.AuthorizationCode, GrantType.ClientCredentials };

        public static ICollection<string> Hybrid =>
            new[] { GrantType.Hybrid };

        public static ICollection<string> HybridAndClientCredentials =>
            new[] { GrantType.Hybrid, GrantType.ClientCredentials };

        public static ICollection<string> ClientCredentials =>
            new[] { GrantType.ClientCredentials };

        public static ICollection<string> ResourceOwnerPassword =>
            new[] { GrantType.ResourceOwnerPassword };

        public static ICollection<string> ResourceOwnerPasswordAndClientCredentials =>
            new[] { GrantType.ResourceOwnerPassword, GrantType.ClientCredentials };

        public static ICollection<string> DeviceFlow =>
            new[] { GrantType.DeviceFlow };
    }
}