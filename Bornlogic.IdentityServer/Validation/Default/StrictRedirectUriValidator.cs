using System.Text.RegularExpressions;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Validation.Default
{
    /// <summary>
    /// Default implementation of redirect URI validator. Validates the URIs against
    /// the client's configured URIs.
    /// </summary>
    public class StrictRedirectUriValidator : IRedirectUriValidator
    {
        /// <summary>
        /// Checks if a given URI string is in a collection of strings (using ordinal ignore case comparison). Supports wildcard validation.
        /// </summary>
        /// <param name="uris">The uris.</param>
        /// <param name="requestedUri">The requested URI.</param>
        /// <returns></returns>
        protected bool UriCollectionContainsUri(IList<string> uris, string requestedUri)
        {
            if (uris.IsNullOrEmpty()) return false;

            var wildCardUris = uris.Where(a => a.Contains('*')).ToList();
            var defaultUris = uris.Where(a => !a.Contains('*')).ToList();

            return defaultUris.Contains(requestedUri, StringComparer.OrdinalIgnoreCase) || 
                   wildCardUris.Select(wildCardUri => "^" + Regex.Escape(wildCardUri).Replace("\\*", ".*") + "$").Any(regex => Regex.IsMatch(requestedUri, regex, RegexOptions.IgnoreCase));
        }

        /// <summary>
        /// Determines whether a redirect URI is valid for a client.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        ///   <c>true</c> is the URI is valid; <c>false</c> otherwise.
        /// </returns>
        public virtual Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(UriCollectionContainsUri(client.RedirectUris?.ToList(), requestedUri));
        }

        /// <summary>
        /// Determines whether a post logout URI is valid for a client.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        ///   <c>true</c> is the URI is valid; <c>false</c> otherwise.
        /// </returns>
        public virtual Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(UriCollectionContainsUri(client.PostLogoutRedirectUris?.ToList(), requestedUri));
        }
    }
}
