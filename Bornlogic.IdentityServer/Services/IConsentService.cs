﻿using System.Security.Claims;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.Services
{
    /// <summary>
    /// Service to retrieve and update consent.
    /// </summary>
    public interface IConsentService
    {
        /// <summary>
        /// Checks if consent is required.
        /// </summary>
        /// <param name="subject">The user.</param>
        /// <param name="client">The client.</param>
        /// <param name="parsedScopes">The parsed scopes.</param>
        /// <returns>
        /// Boolean if consent is required.
        /// </returns>
        Task<bool> RequiresConsentAsync(ClaimsPrincipal subject, Client client, IEnumerable<ParsedScopeValue> parsedScopes);

        /// <summary>
        /// Updates the consent.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <param name="parsedScopes">The parsed scopes.</param>
        /// <returns></returns>
        Task UpdateConsentAsync(ClaimsPrincipal subject, Client client, IEnumerable<ParsedScopeValue> parsedScopes);
    }
}