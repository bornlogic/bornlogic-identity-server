﻿using Bornlogic.IdentityServer.Models;
using Bornlogic.IdentityServer.Validation.Contexts;
using Bornlogic.IdentityServer.Validation.Models;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Validation.Default
{
    /// <summary>
    /// Validates an extension grant request using the registered validators
    /// </summary>
    public class ExtensionGrantValidator
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IExtensionGrantValidator> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionGrantValidator"/> class.
        /// </summary>
        /// <param name="validators">The validators.</param>
        /// <param name="logger">The logger.</param>
        public ExtensionGrantValidator(IEnumerable<IExtensionGrantValidator> validators, ILogger<ExtensionGrantValidator> logger)
        {
            if (validators == null)
            {
                _validators = Enumerable.Empty<IExtensionGrantValidator>();
            }
            else
            {
                _validators = validators;
            }

            _logger = logger;
        }

        /// <summary>
        /// Gets the available grant types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAvailableGrantTypes()
        {
            return _validators.Select(v => v.GrantType);
        }

        /// <summary>
        /// Validates the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<GrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            var validator = _validators.FirstOrDefault(v => v.GrantType.Equals(request.GrantType, StringComparison.Ordinal));

            if (validator == null)
            {
                _logger.LogError("No validator found for grant type");
                return new GrantValidationResult(TokenRequestErrors.UnsupportedGrantType);
            }

            try
            {
                _logger.LogTrace("Calling into custom grant validator: {type}", validator.GetType().FullName);

                var context = new ExtensionGrantValidationContext
                {
                    Request = request
                };
            
                await validator.ValidateAsync(context);
                return context.Result;
            }
            catch (Exception e)
            {
                _logger.LogError(1, e, "Grant validation error: {message}", e.Message);
                return new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            }
        }
    }
}