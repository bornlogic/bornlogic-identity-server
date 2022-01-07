// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Http;

namespace Bornlogic.IdentityServer.Storage.Services
{
    /// <summary>
    /// Service that determines if CORS is allowed.
    /// </summary>
    public interface ICorsPolicyService
    {
        /// <summary>
        /// Determines whether origin is allowed.
        /// </summary>
        /// <param name="httpContext">The http context</param>
        /// <returns></returns>
        Task<bool> IsOriginAllowedAsync(HttpContext httpContext);
    }
}
