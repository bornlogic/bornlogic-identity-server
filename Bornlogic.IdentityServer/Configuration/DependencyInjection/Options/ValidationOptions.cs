namespace Bornlogic.IdentityServer.Configuration.DependencyInjection.Options
{
    /// <summary>
    /// The ValidationOptions contains settings that affect some of the default validation behavior.
    /// </summary>
    public class ValidationOptions
    {
        /// <summary>
        ///  Collection of URI scheme prefixes that should never be used as custom URI schemes in the redirect_uri passed to tha authorize endpoint.
        /// </summary>
        public ICollection<string> InvalidRedirectUriPrefixes { get; } = new HashSet<string>
        {
            "javascript:",
            "file:",
            "data:",
            "mailto:",
            "ftp:",
            "blob:",
            "about:",
            "ssh:",
            "tel:",
            "view-source:",
            "ws:",
            "wss:"
        };
    }
}