



namespace Bornlogic.IdentityServer.Events.Infrastructure
{
    /// <summary>
    /// Categories for events
    /// </summary>
    public static class EventCategories
    {
        /// <summary>
        /// Authentication related events
        /// </summary>
        public const string Authentication = "Authentication";

        /// <summary>
        /// Token related events
        /// </summary>
        public const string Token = "Token";

        /// <summary>
        /// Grants related events
        /// </summary>
        public const string Grants = "Grants";

        /// <summary>
        /// Error related events
        /// </summary>
        public const string Error = "Error";

        /// <summary>
        /// Device flow related events
        /// </summary>
        public const string DeviceFlow = "Device";
    }
}