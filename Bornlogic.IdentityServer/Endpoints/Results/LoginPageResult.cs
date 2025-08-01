using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Hosting;
using Bornlogic.IdentityServer.Models.Messages;
using Bornlogic.IdentityServer.Stores;
using Bornlogic.IdentityServer.Validation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bornlogic.IdentityServer.Endpoints.Results
{
    /// <summary>
    /// Result for login page
    /// </summary>
    /// <seealso cref="IEndpointResult" />
    public class LoginPageResult : IEndpointResult
    {
        private readonly ValidatedAuthorizeRequest _request;
        private readonly IDictionary<string, string> _additionalQueryParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPageResult"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <exception cref="System.ArgumentNullException">request</exception>
        public LoginPageResult(ValidatedAuthorizeRequest request, IDictionary<string, string> additionalQueryParameters)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _additionalQueryParameters = additionalQueryParameters;
        }

        internal LoginPageResult(
            ValidatedAuthorizeRequest request,
            IdentityServerOptions options,
            IAuthorizationParametersMessageStore authorizationParametersMessageStore = null,
            IDictionary<string, string> additionalQueryParameters = null) 
            : this(request, additionalQueryParameters)
        {
            _options = options;
            _authorizationParametersMessageStore = authorizationParametersMessageStore;
        }

        private IdentityServerOptions _options;
        private IAuthorizationParametersMessageStore _authorizationParametersMessageStore;

        private void Init(HttpContext context)
        {
            _options = _options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _authorizationParametersMessageStore = _authorizationParametersMessageStore ?? context.RequestServices.GetService<IAuthorizationParametersMessageStore>();
        }

        /// <summary>
        /// Executes the result.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            var returnUrl = context.GetIdentityServerBasePath().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.AuthorizeCallback;
            if (_authorizationParametersMessageStore != null)
            {
                var msg = new Message<IDictionary<string, string[]>>(_request.Raw.ToFullDictionary());
                var id = await _authorizationParametersMessageStore.WriteAsync(msg);
                returnUrl = returnUrl.AddQueryString(Constants.AuthorizationParamsStore.MessageStoreIdParameterName, id);
            }
            else
            {
                returnUrl = returnUrl.AddQueryString(_request.Raw.ToQueryString());
            }

            var loginUrl = _options.UserInteraction.LoginUrl;
            if (!loginUrl.IsLocalUrl())
            {
                // this converts the relative redirect path to an absolute one if we're 
                // redirecting to a different server
                returnUrl = context.GetIdentityServerHost().EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
            }

            var url = loginUrl.AddQueryString(_options.UserInteraction.LoginReturnUrlParameter, returnUrl);

            foreach (var additionalQueryParameter in _additionalQueryParameters ?? new Dictionary<string, string>())
            {
                url = url.AddQueryString(additionalQueryParameter.Key, additionalQueryParameter.Value);
            }

            context.Response.RedirectToAbsoluteUrl(url);
        }
    }
}