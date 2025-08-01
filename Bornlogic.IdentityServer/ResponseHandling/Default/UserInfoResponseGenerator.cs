using System.Security.Claims;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Models.Contexts;
using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;
using Bornlogic.IdentityServer.Validation.Models;
using IdentityModel;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.ResponseHandling.Default
{
    /// <summary>
    /// The userinfo response generator
    /// </summary>
    /// <seealso cref="IUserInfoResponseGenerator" />
    public class UserInfoResponseGenerator : IUserInfoResponseGenerator
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// The user claims enricher
        /// </summary>
        protected readonly IUserInfoClaimsEnricher UserInfoClaimsEnricher;

        /// <summary>
        /// The profile service
        /// </summary>
        protected readonly IProfileService Profile;

        /// <summary>
        /// The resource store
        /// </summary>
        protected readonly IResourceStore Resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfoResponseGenerator"/> class.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <param name="resourceStore">The resource store.</param>
        /// <param name="logger">The logger.</param>
        public UserInfoResponseGenerator
            (
                IProfileService profile, 
                IResourceStore resourceStore, 
                ILogger<UserInfoResponseGenerator> logger,
                IUserInfoClaimsEnricher userInfoClaimsEnricher
            )
        {
            Profile = profile;
            Resources = resourceStore;
            Logger = logger;
            UserInfoClaimsEnricher = userInfoClaimsEnricher;
        }

        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <param name="validationResult">The userinfo request validation result.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Profile service returned incorrect subject value</exception>
        public virtual async Task<Dictionary<string, object>> ProcessAsync(UserInfoRequestValidationResult validationResult)
        {
            Logger.LogDebug("Creating userinfo response");

            // extract scopes and turn into requested claim types
            var scopes = validationResult.TokenValidationResult.Claims.Where(c => c.Type == JwtClaimTypes.Scope).Select(c => c.Value);

            var validatedResources = await GetRequestedResourcesAsync(scopes);
            var requestedClaimTypes = await GetRequestedClaimTypesAsync(validatedResources);

            Logger.LogDebug("Requested claim types: {claimTypes}", requestedClaimTypes.ToSpaceSeparatedString());

            // call profile service
            var context = new ProfileDataRequestContext(
                validationResult.Subject,
                validationResult.TokenValidationResult.Client,
                IdentityServerConstants.ProfileDataCallers.UserInfoEndpoint,
                requestedClaimTypes);
            context.RequestedResources = validatedResources;

            await Profile.GetProfileDataAsync(context);
            var profileClaims = context.IssuedClaims;

            // construct outgoing claims
            var outgoingClaims = new List<Claim>();

            if (profileClaims == null)
            {
                Logger.LogInformation("Profile service returned no claims (null)");
            }
            else
            {
                outgoingClaims.AddRange(profileClaims);
                Logger.LogInformation("Profile service returned the following claim types: {types}", profileClaims.Select(c => c.Type).ToSpaceSeparatedString());
            }

            var subClaim = outgoingClaims.SingleOrDefault(x => x.Type == JwtClaimTypes.Subject);
            if (subClaim == null)
            {
                outgoingClaims.Add(new Claim(JwtClaimTypes.Subject, validationResult.Subject.GetSubjectId()));
            }
            else if (subClaim.Value != validationResult.Subject.GetSubjectId())
            {
                Logger.LogError("Profile service returned incorrect subject value: {sub}", subClaim);
                throw new InvalidOperationException("Profile service returned incorrect subject value");
            }

            var dictionary = outgoingClaims.ToClaimsDictionary();

            var additionalClaims = await UserInfoClaimsEnricher.GetAdditionalClaims(context?.Subject);

            foreach (var additionalClaim in additionalClaims ?? [])
            {
                if(dictionary.ContainsKey(additionalClaim.Type))
                    dictionary[additionalClaim.Type] = additionalClaim.Value;
                else 
                    dictionary.Add(additionalClaim.Type, additionalClaim.Value);
            }

            return dictionary;
        }

        /// <summary>
        ///  Gets the identity resources from the scopes.
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns></returns>
        protected internal virtual async Task<ResourceValidationResult> GetRequestedResourcesAsync(IEnumerable<string> scopes)
        {
            if (scopes == null || !scopes.Any())
            {
                return null;
            }

            var scopeString = string.Join(" ", scopes);
            Logger.LogDebug("Scopes in access token: {scopes}", scopeString);

            // if we ever parameterize identity scopes, then we would need to invoke the resource validator's parse API here
            var identityResources = await Resources.FindEnabledIdentityResourcesByScopeAsync(scopes);
            
            var resources = new Resources(identityResources, Enumerable.Empty<ApiResource>(), Enumerable.Empty<ApiScope>());
            var result = new ResourceValidationResult(resources);
            
            return result;
        }

        /// <summary>
        /// Gets the requested claim types.
        /// </summary>
        /// <param name="resourceValidationResult"></param>
        /// <returns></returns>
        protected internal virtual Task<IEnumerable<string>> GetRequestedClaimTypesAsync(ResourceValidationResult resourceValidationResult)
        {
            IEnumerable<string> result = null;

            if (resourceValidationResult == null)
            {
                result = Enumerable.Empty<string>();
            }
            else
            {
                var identityResources = resourceValidationResult.Resources.IdentityResources;
                result = identityResources.SelectMany(x => x.UserClaims).Distinct();
            }

            return Task.FromResult(result);
        }
    }
}