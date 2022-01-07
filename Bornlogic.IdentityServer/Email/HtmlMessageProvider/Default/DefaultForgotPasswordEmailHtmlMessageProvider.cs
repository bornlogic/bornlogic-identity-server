using System.Text.Encodings.Web;
using Bornlogic.IdentityServer.Email.HtmlMessageProvider.Contracts;

namespace Bornlogic.IdentityServer.Email.HtmlMessageProvider.Default
{
    public class DefaultForgotPasswordEmailHtmlMessageProvider : IForgotPasswordEmailHtmlMessageProvider
    {
        public Task<KeyValuePair<string, string>> GetSubjectAndHtmlMessage(string userName, string callbackUrl)
        {
            var hasUserName = !string.IsNullOrEmpty(userName);

            return Task.FromResult(
                new KeyValuePair<string, string>
                (
                    "Forgot Password",
                    hasUserName 
                        ? $"Hi, {userName}! <br/> Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>." 
                        : $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.")
                );
        }
    }
}
