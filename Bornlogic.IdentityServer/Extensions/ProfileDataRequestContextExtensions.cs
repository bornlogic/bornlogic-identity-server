using System.Security.Claims;
using Bornlogic.IdentityServer.Models.Contexts;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Extensions
{
    /// <summary>
    /// Extensions for ProfileDataRequestContext
    /// </summary>
    public static class ProfileDataRequestContextExtensions
    {
        /// <summary>
        /// Filters the claims based on requested claim types.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public static List<Claim> FilterClaims(this ProfileDataRequestContext context, IEnumerable<Claim> claims)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            return claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
        }

        /// <summary>
        /// Filters the claims based on the requested claim types and then adds them to the IssuedClaims collection.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="claims">The claims.</param>
        public static void AddRequestedClaims(this ProfileDataRequestContext context, IEnumerable<Claim> claims)
        {
            if (context.RequestedClaimTypes.Any())
            {
                context.IssuedClaims.AddRange(context.FilterClaims(claims));
            }
        }

        /// <summary>
        /// Logs the profile request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public static void LogProfileRequest(this ProfileDataRequestContext context, ILogger logger)
        {
            logger.LogDebug("Get profile called for subject {subject} from client {client} with claim types {claimTypes} via {caller}",
                context.Subject.GetSubjectId(),
                context.Client.ClientName ?? context.Client.ClientId,
                context.RequestedClaimTypes,
                context.Caller);
        }

        /// <summary>
        /// Logs the issued claims.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public static void LogIssuedClaims(this ProfileDataRequestContext context, ILogger logger)
        {
            logger.LogDebug("Issued claims: {claims}", context.IssuedClaims.Select(c => c.Type));
        }
    }
}