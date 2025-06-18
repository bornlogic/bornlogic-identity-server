namespace Bornlogic.IdentityServer.Validation.Models
{
    /// <summary>
    /// Minimal validation result class (base-class for more complext validation results)
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the validation was successful.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the validation is failed; otherwise, <c>false</c>.
        /// </value>
        public bool IsError { get; set; } = true;

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the sub error.
        /// </summary>
        /// <value>
        /// The sub error.
        /// </value>
        public string SubError { get; set; }
    }
}