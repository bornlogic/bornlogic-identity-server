﻿using System.Collections.Specialized;
using System.Security.Claims;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.Validation
{
    /// <summary>
    ///  Authorize endpoint request validator.
    /// </summary>
    public interface IAuthorizeRequestValidator
    {
        /// <summary>
        ///  Validates authorize request parameters.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject = null);
    }
}