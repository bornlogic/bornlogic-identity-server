﻿using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Validation
{
    /// <summary>
    /// Models the logic when validating redirect and post logout redirect URIs.
    /// </summary>
    public interface IRedirectUriValidator
    {
        /// <summary>
        /// Determines whether a redirect URI is valid for a client.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="client">The client.</param>
        /// <returns><c>true</c> is the URI is valid; <c>false</c> otherwise.</returns>
        Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client);
        
        /// <summary>
        /// Determines whether a post logout URI is valid for a client.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="client">The client.</param>
        /// <returns><c>true</c> is the URI is valid; <c>false</c> otherwise.</returns>
        Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client);
    }
}