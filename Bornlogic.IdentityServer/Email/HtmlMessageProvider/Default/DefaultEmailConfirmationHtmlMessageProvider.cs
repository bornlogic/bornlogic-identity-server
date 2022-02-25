using Bornlogic.IdentityServer.Culture.Contracts;
using Bornlogic.IdentityServer.Email.HtmlMessageProvider.Contracts;
using System.Text.Encodings.Web;

namespace Bornlogic.IdentityServer.Email.HtmlMessageProvider.Default
{
    public class DefaultEmailConfirmationProvider : IEmailConfirmationHtmlMessageProvider
    {
        private const string EMAIL_CONFIRMATION_SUBJECT_KEY = "Email_Confirmation_Subject";
        private const string EMAIL_CONFIRMATION_SUBJECT_USERNAME_KEY = "Email_Confirmation_Subject_Username";
        private const string EMAIL_CONFIRMATION_MESSAGE_KEY = "Email_Confirmation_Message";
        private const string EMAIL_CONFIRMATION_MESSAGE_USERNAME_KEY = "Email_Confirmation_Message_Username";

        private readonly IEmailResourceProvider _emailResourceProvider;

        public DefaultEmailConfirmationProvider(IEmailResourceProvider emailResourceProvider)
        {
            _emailResourceProvider = emailResourceProvider;
        }

        public Task<KeyValuePair<string, string>> GetSubjectAndHtmlMessage(string userName, string callbackUrl)
        {
            var hasUserName = !string.IsNullOrEmpty(userName);

            return Task.FromResult(
                new KeyValuePair<string, string>
                (
                    hasUserName
                        ? string.Format(_emailResourceProvider.GetStringOrDefault(EMAIL_CONFIRMATION_SUBJECT_USERNAME_KEY, "{0}, confirm your email"), userName)
                        : _emailResourceProvider.GetStringOrDefault(EMAIL_CONFIRMATION_SUBJECT_KEY, "Confirm your email"),
                    hasUserName
                        ? string.Format(_emailResourceProvider.GetStringOrDefault(
                            EMAIL_CONFIRMATION_MESSAGE_USERNAME_KEY, "Hi, {0}! <br/> Please confirm your account by <a href='{1}'>clicking here</a>."), userName, HtmlEncoder.Default.Encode(callbackUrl))
                        : string.Format(_emailResourceProvider.GetStringOrDefault(
                            EMAIL_CONFIRMATION_MESSAGE_KEY, "Please confirm your account by <a href='{0}'>clicking here</a>."), HtmlEncoder.Default.Encode(callbackUrl)))
            );
        }
    }
}
