// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Bornlogic.IdentityServer.Validation.Models
{
    /// <summary>
    /// Validation result for authorize requests
    /// </summary>
    public class AuthorizeRequestValidationResult : ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeRequestValidationResult"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public AuthorizeRequestValidationResult(ValidatedAuthorizeRequest request)
        {
            ValidatedRequest = request;
            IsError = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeRequestValidationResult" /> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="error">The error.</param>
        /// <param name="subError">The sub error.</param>
        public AuthorizeRequestValidationResult(ValidatedAuthorizeRequest request, string error, string subError = null)
        {
            ValidatedRequest = request;
            IsError = true;
            Error = error;
            SubError = subError;
        }

        /// <summary>
        /// Gets or sets the validated request.
        /// </summary>
        /// <value>
        /// The validated request.
        /// </value>
        public ValidatedAuthorizeRequest ValidatedRequest { get; }
    }
}