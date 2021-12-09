using System.Text.Encodings.Web;
using Bornlogic.IdentityServer.Email.HtmlMessageProvider.Contracts;

namespace Bornlogic.IdentityServer.Email.HtmlMessageProvider.Default
{
    public class DefaultEmailConfirmationProvider : IEmailConfirmationProvider
    {
        public Task<KeyValuePair<string, string>> GetSubjectAndHtmlMessage(string userName, string callbackUrl)
        {
            var hasUserName = !string.IsNullOrEmpty(userName);

            return Task.FromResult(
                new KeyValuePair<string, string>
                (
                    hasUserName 
                        ? $"{userName}, confirm your email" 
                        : "Confirm your email",
                    hasUserName 
                        ? $"Hi, {userName}! <br/> Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>." 
                        : $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.")
                );
        }
    }
}
