using Bornlogic.IdentityServer.Configuration.DependencyInjection.Options;
using Bornlogic.IdentityServer.Hosting;
using Bornlogic.IdentityServer.Models.Messages;
using Bornlogic.IdentityServer.Stores;
using Bornlogic.IdentityServer.Validation.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bornlogic.IdentityServer.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Bornlogic.IdentityServer.Endpoints.Results
{
    public class BusinessSelectPageResult : IEndpointResult
    {
        private readonly ValidatedAuthorizeRequest _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentPageResult"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <exception cref="System.ArgumentNullException">request</exception>
        public BusinessSelectPageResult(ValidatedAuthorizeRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        internal BusinessSelectPageResult(
            ValidatedAuthorizeRequest request,
            IdentityServerOptions options,
            IAuthorizationParametersMessageStore authorizationParametersMessageStore = null)
            : this(request)
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

            var businessSelectUrl = _options.UserInteraction.BusinessSelect;
            if (!businessSelectUrl.IsLocalUrl())
            {
                // this converts the relative redirect path to an absolute one if we're 
                // redirecting to a different server
                returnUrl = context.GetIdentityServerHost().EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
            }

            var url = businessSelectUrl.AddQueryString(_options.UserInteraction.ConsentReturnUrlParameter, returnUrl);
            context.Response.RedirectToAbsoluteUrl(url);
        }
    }
}
