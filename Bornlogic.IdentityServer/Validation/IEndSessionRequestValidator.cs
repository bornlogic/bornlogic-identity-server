using System.Collections.Specialized;
using System.Security.Claims;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.Validation
{
    /// <summary>
    ///  Validates end session requests.
    /// </summary>
    public interface IEndSessionRequestValidator
    {
        /// <summary>
        /// Validates end session endpoint requests.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        Task<EndSessionValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject);

        /// <summary>
        ///  Validates requests from logout page iframe to trigger single signout.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<EndSessionCallbackValidationResult> ValidateCallbackAsync(NameValueCollection parameters);
    }
}
