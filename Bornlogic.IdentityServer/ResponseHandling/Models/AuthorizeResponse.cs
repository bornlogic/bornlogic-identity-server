﻿using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Validation.Models;

#pragma warning disable 1591

namespace Bornlogic.IdentityServer.ResponseHandling.Models
{
    public class AuthorizeResponse
    {
        public ValidatedAuthorizeRequest Request { get; set; }
        public string RedirectUri => Request?.RedirectUri;
        public string State => Request?.State;
        public string Scope => Request?.ValidatedResources?.RawScopeValues.ToSpaceSeparatedString();

        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public int AccessTokenLifetime { get; set; }
        public string Code { get; set; }
        public string SessionState { get; set; }

        public string Error { get; set; }
        public string SubError { get; set; }
        public bool IsError => Error.IsPresent();
    }
}