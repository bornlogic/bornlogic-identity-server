namespace Bornlogic.IdentityServer.Models.Messages.Enums
{
    /// <summary>
    /// Enum to model interaction authorization errors.
    /// </summary>
    public enum AuthorizationError
    {
        /// <summary>
        /// Access denied
        /// </summary>
        AccessDenied,

        /// <summary>
        /// Interaction required
        /// </summary>
        InteractionRequired,

        /// <summary>
        /// Login required
        /// </summary>
        LoginRequired,

        /// <summary>
        /// Account selection required
        /// </summary>
        AccountSelectionRequired,

        /// <summary>
        /// Consent required
        /// </summary>
        ConsentRequired,
        
        BusinessRequired,

        ConfirmedEmailRequired,

        TosAcceptanceRequired
    }
}
