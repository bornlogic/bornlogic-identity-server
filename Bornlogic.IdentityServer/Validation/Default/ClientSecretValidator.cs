using Bornlogic.IdentityServer.Events;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Storage.Stores;
using Bornlogic.IdentityServer.Validation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Validation.Default
{
    /// <summary>
    /// Validates a client secret using the registered secret validators and parsers
    /// </summary>
    public class ClientSecretValidator : IClientSecretValidator
    {
        private readonly ILogger _logger;
        private readonly IClientUserRoleService _clientUserRoleService;
        private readonly IClientStore _clients;
        private readonly IEventService _events;
        private readonly ISecretsListValidator _validator;
        private readonly ISecretsListParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSecretValidator"/> class.
        /// </summary>
        /// <param name="clients">The clients.</param>
        /// <param name="parser">The parser.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        public ClientSecretValidator(IClientStore clients, ISecretsListParser parser, ISecretsListValidator validator, IEventService events, ILogger<ClientSecretValidator> logger, IClientUserRoleService clientUserRoleService)
        {
            _clients = clients;
            _parser = parser;
            _validator = validator;
            _events = events;
            _logger = logger;
            _clientUserRoleService = clientUserRoleService;
        }

        /// <summary>
        /// Validates the current request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<ClientSecretValidationResult> ValidateAsync(HttpContext context)
        {
            _logger.LogDebug("Start client validation");

            var fail = new ClientSecretValidationResult
            {
                IsError = true
            };

            var parsedSecret = await _parser.ParseAsync(context);
            if (parsedSecret == null)
            {
                await RaiseFailureEventAsync("unknown", "No client id found");

                _logger.LogError("No client identifier found");
                return fail;
            }

            // load client
            var client = await _clients.FindClientByIdAsync(parsedSecret.Id);
            if (client == null)
            {
                await RaiseFailureEventAsync(parsedSecret.Id, "Unknown client");

                _logger.LogError("No client with id '{clientId}' found. aborting", parsedSecret.Id);
                return fail;
            }

            SecretValidationResult secretValidationResult = null;
            if (!client.RequireClientSecret || client.IsImplicitOnly())
            {
                _logger.LogDebug("Public Client - skipping secret validation success");
            }
            else
            {
                secretValidationResult = await _validator.ValidateAsync(client.ClientSecrets, parsedSecret);
                if (secretValidationResult.Success == false)
                {
                    await RaiseFailureEventAsync(client.ClientId, "Invalid client secret");
                    _logger.LogError("Client secret validation failed for client: {clientId}.", client.ClientId);

                    return fail;
                }
            }

            _logger.LogDebug("Client validation success");

            var success = new ClientSecretValidationResult
            {
                IsError = false,
                Client = client,
                Secret = parsedSecret,
                Confirmation = secretValidationResult?.Confirmation
            };

            await RaiseSuccessEventAsync(client.ClientId, parsedSecret.Type);
            return success;
        }

        private Task RaiseSuccessEventAsync(string clientId, string authMethod)
        {
            return _events.RaiseAsync(new ClientAuthenticationSuccessEvent(clientId, authMethod));
        }

        private Task RaiseFailureEventAsync(string clientId, string message)
        {
            return _events.RaiseAsync(new ClientAuthenticationFailureEvent(clientId, message));
        }
    }
}