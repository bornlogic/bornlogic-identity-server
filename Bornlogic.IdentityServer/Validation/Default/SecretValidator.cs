﻿using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Models;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Validation.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Validation.Default
{
    /// <summary>
    /// Validates secrets using the registered validators
    /// </summary>
    public class SecretValidator : ISecretsListValidator
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<ISecretValidator> _validators;
        private readonly ISystemClock _clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretValidator"/> class.
        /// </summary>
        /// <param name="clock">The clock.</param>
        /// <param name="validators">The validators.</param>
        /// <param name="logger">The logger.</param>
        public SecretValidator(ISystemClock clock, IEnumerable<ISecretValidator> validators, ILogger<ISecretsListValidator> logger)
        {
            _clock = clock;
            _validators = validators;
            _logger = logger;
        }

        /// <summary>
        /// Validates the secret.
        /// </summary>
        /// <param name="parsedSecret">The parsed secret.</param>
        /// <param name="secrets">The secrets.</param>
        /// <returns></returns>
        public async Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var secretsArray = secrets as Secret[] ?? secrets.ToArray();

            var expiredSecrets = secretsArray.Where(s => s.Expiration.HasExpired(_clock.UtcNow.UtcDateTime)).ToList();
            if (expiredSecrets.Any())
            {
                expiredSecrets.ForEach(
                    ex => _logger.LogInformation("Secret [{description}] is expired", ex.Description ?? "no description"));
            }

            var currentSecrets = secretsArray.Where(s => !s.Expiration.HasExpired(_clock.UtcNow.UtcDateTime)).ToArray();

            // see if a registered validator can validate the secret
            foreach (var validator in _validators)
            {
                var secretValidationResult = await validator.ValidateAsync(currentSecrets, parsedSecret);

                if (secretValidationResult.Success)
                {
                    _logger.LogDebug("Secret validator success: {0}", validator.GetType().Name);
                    return secretValidationResult;
                }
            }

            _logger.LogDebug("Secret validators could not validate secret");
            return new SecretValidationResult { Success = false };
        }
    }
}
