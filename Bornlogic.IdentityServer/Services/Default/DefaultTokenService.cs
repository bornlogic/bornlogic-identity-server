using System.Security.Claims;
using Bornlogic.IdentityServer.Configuration;
using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Models;
using Bornlogic.IdentityServer.Models.Messages.Enums;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;
using Bornlogic.IdentityServer.Validation.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Services.Default
{
    /// <summary>
    /// Default token service
    /// </summary>
    public class DefaultTokenService : ITokenService
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// The HTTP context accessor
        /// </summary>
        protected readonly IHttpContextAccessor ContextAccessor;

        /// <summary>
        /// The claims provider
        /// </summary>
        protected readonly IClaimsService ClaimsProvider;

        /// <summary>
        /// The reference token store
        /// </summary>
        protected readonly IReferenceTokenStore ReferenceTokenStore;

        /// <summary>
        /// The signing service
        /// </summary>
        protected readonly ITokenCreationService CreationService;

        /// <summary>
        /// The clock
        /// </summary>
        protected readonly ISystemClock Clock;

        /// <summary>
        /// The key material service
        /// </summary>
        protected readonly IKeyMaterialService KeyMaterialService;

        /// <summary>
        /// The IdentityServer options
        /// </summary>
        protected readonly IdentityServerOptions Options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenService" /> class.
        /// </summary>
        /// <param name="claimsProvider">The claims provider.</param>
        /// <param name="referenceTokenStore">The reference token store.</param>
        /// <param name="creationService">The signing service.</param>
        /// <param name="contextAccessor">The HTTP context accessor.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="keyMaterialService"></param>
        /// <param name="options">The IdentityServer options</param>
        /// <param name="logger">The logger.</param>
        public DefaultTokenService(
            IClaimsService claimsProvider,
            IReferenceTokenStore referenceTokenStore,
            ITokenCreationService creationService,
            IHttpContextAccessor contextAccessor,
            ISystemClock clock,
            IKeyMaterialService keyMaterialService,
            IdentityServerOptions options,
            ILogger<DefaultTokenService> logger)
        {
            ContextAccessor = contextAccessor;
            ClaimsProvider = claimsProvider;
            ReferenceTokenStore = referenceTokenStore;
            CreationService = creationService;
            Clock = clock;
            KeyMaterialService = keyMaterialService;
            Options = options;
            Logger = logger;
        }

        /// <summary>
        /// Creates an identity token.
        /// </summary>
        /// <param name="request">The token creation request.</param>
        /// <returns>
        /// An identity token
        /// </returns>
        public virtual async Task<Token> CreateIdentityTokenAsync(TokenCreationRequest request)
        {
            Logger.LogTrace("Creating identity token");
            request.Validate();

            // todo: Dom, add a test for this. validate the at and c hashes are correct for the id_token when the client's alg doesn't match the server default.
            var credential = await KeyMaterialService.GetSigningCredentialsAsync(request.ValidatedRequest.Client.AllowedIdentityTokenSigningAlgorithms);
            if (credential == null)
            {
                throw new InvalidOperationException("No signing credential is configured.");
            }

            var signingAlgorithm = credential.Algorithm;

            // host provided claims
            var claims = new List<Claim>();

            // if nonce was sent, must be mirrored in id token
            if (request.Nonce.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.Nonce, request.Nonce));
            }

            // add iat claim
            claims.Add(new Claim(JwtClaimTypes.IssuedAt, Clock.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            // add at_hash claim
            if (request.AccessTokenToHash.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.AccessTokenHash, CryptoHelper.CreateHashClaimValue(request.AccessTokenToHash, signingAlgorithm)));
            }

            // add c_hash claim
            if (request.AuthorizationCodeToHash.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.AuthorizationCodeHash, CryptoHelper.CreateHashClaimValue(request.AuthorizationCodeToHash, signingAlgorithm)));
            }

            // add s_hash claim
            if (request.StateHash.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.StateHash, request.StateHash));
            }

            // add sid if present
            if (request.ValidatedRequest.SessionId.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.SessionId, request.ValidatedRequest.SessionId));
            }

            claims.AddRange(await ClaimsProvider.GetIdentityTokenClaimsAsync(
                request.Subject,
                request.ValidatedResources,
                request.IncludeAllIdentityClaims,
                request.ValidatedRequest));

            var issuer = ContextAccessor.HttpContext.GetIdentityServerIssuerUri();

            var token = new Token(OidcConstants.TokenTypes.IdentityToken)
            {
                CreationTime = Clock.UtcNow.UtcDateTime,
                Audiences = { request.ValidatedRequest.Client.ClientId },
                Issuer = issuer,
                Lifetime = request.ValidatedRequest.Client.IdentityTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                ClientId = request.ValidatedRequest.Client.ClientId,
                AccessTokenType = request.ValidatedRequest.AccessTokenType,
                AllowedSigningAlgorithms = request.ValidatedRequest.Client.AllowedIdentityTokenSigningAlgorithms
            };

            return token;
        }

        /// <summary>
        /// Creates an access token.
        /// </summary>
        /// <param name="request">The token creation request.</param>
        /// <returns>
        /// An access token
        /// </returns>
        public virtual async Task<Token> CreateAccessTokenAsync(TokenCreationRequest request)
        {
            Logger.LogTrace("Creating access token");
            request.Validate();

            var claims = new List<Claim>();
            claims.AddRange(await ClaimsProvider.GetAccessTokenClaimsAsync(
                request.Subject,
                request.ValidatedResources,
                request.ValidatedRequest));

            if (request.ValidatedRequest is ValidatedTokenRequest tokenRequest && tokenRequest.AuthorizationCode != null)
            {
                if (!string.IsNullOrEmpty(tokenRequest.AuthorizationCode.BusinessId))
                    claims.Add(new Claim(BusinessScopedClaims.BusinessId, tokenRequest.AuthorizationCode.BusinessId));

                if (!string.IsNullOrEmpty(tokenRequest.AuthorizationCode.SubjectBusinessScopedId))
                    claims.Add(new Claim(BusinessScopedClaims.SubjectBusinessScopedId, tokenRequest.AuthorizationCode.SubjectBusinessScopedId));
            }

            if (request.ValidatedRequest.Client.IncludeJwtId)
            {
                claims.Add(new Claim(JwtClaimTypes.JwtId, CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex)));
            }

            if (request.ValidatedRequest.SessionId.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.SessionId, request.ValidatedRequest.SessionId));
            }

            // iat claim as required by JWT profile
            claims.Add(new Claim(JwtClaimTypes.IssuedAt, Clock.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64));

            var issuer = !string.IsNullOrEmpty(request.Issuer) ? request.Issuer : ContextAccessor.HttpContext.GetIdentityServerIssuerUri();
            var token = new Token(OidcConstants.TokenTypes.AccessToken)
            {
                CreationTime = Clock.UtcNow.UtcDateTime,
                Issuer = issuer,
                Lifetime = request.ValidatedRequest.AccessTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                ClientId = request.ValidatedRequest.Client.ClientId,
                Description = request.Description,
                AccessTokenType = request.ValidatedRequest.AccessTokenType,
                AllowedSigningAlgorithms = request.ValidatedResources.Resources.ApiResources.FindMatchingSigningAlgorithms(),
                IgnoreLifetimeValidation = request.IgnoreLifetimeValidation
            };

            // add aud based on ApiResources in the validated request
            foreach (var aud in request.ValidatedResources.Resources.ApiResources.Select(x => x.Name).Distinct())
            {
                token.Audiences.Add(aud);
            }

            if (Options.EmitStaticAudienceClaim)
            {
                token.Audiences.Add(string.Format(IdentityServerConstants.AccessTokenAudience, issuer.EnsureTrailingSlash()));
            }

            // add cnf if present
            if (request.ValidatedRequest.Confirmation.IsPresent())
            {
                token.Confirmation = request.ValidatedRequest.Confirmation;
            }
            else
            {
                if (Options.MutualTls.AlwaysEmitConfirmationClaim)
                {
                    var clientCertificate = await ContextAccessor.HttpContext.Connection.GetClientCertificateAsync();
                    if (clientCertificate != null)
                    {
                        token.Confirmation = clientCertificate.CreateThumbprintCnf();
                    }
                }
            }

            return token;
        }

        /// <summary>
        /// Creates a serialized and protected security token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// A security token in serialized form
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Invalid token type.</exception>
        public virtual async Task<string> CreateSecurityTokenAsync(Token token)
        {
            string tokenResult;

            if (token.Type == OidcConstants.TokenTypes.AccessToken)
            {
                if (token.AccessTokenType == AccessTokenType.Jwt)
                {
                    Logger.LogTrace("Creating JWT access token");

                    tokenResult = await CreationService.CreateTokenAsync(token);
                }
                else
                {
                    Logger.LogTrace("Creating reference access token");

                    var handle = await ReferenceTokenStore.StoreReferenceTokenAsync(token);

                    tokenResult = handle;
                }
            }
            else if (token.Type == OidcConstants.TokenTypes.IdentityToken)
            {
                Logger.LogTrace("Creating JWT identity token");

                tokenResult = await CreationService.CreateTokenAsync(token);
            }
            else
            {
                throw new InvalidOperationException("Invalid token type.");
            }

            return tokenResult;
        }
    }
}