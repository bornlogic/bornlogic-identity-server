namespace Bornlogic.IdentityServer.Models
{
    public class RefreshTokenLifetimeResult
    {
        public int AbsoluteLifetime { get; set; }
        public int SlidingLifetime { get; set; }
    }
}
