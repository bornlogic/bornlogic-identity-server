using System.Text.Encodings.Web;
using Bornlogic.IdentityServer.Culture.Contracts;
using Bornlogic.IdentityServer.Email.HtmlMessageProvider.Contracts;

namespace Bornlogic.IdentityServer.Email.HtmlMessageProvider.Default
{
    public class DefaultForgotPasswordEmailHtmlMessageProvider : IForgotPasswordEmailHtmlMessageProvider
    {
        private const string EMAIL_FORGOT_PASSWORD_SUBJECT_KEY = "Email_Forgot_Password_Subject";
        private const string EMAIL_FORGOT_PASSWORD_MESSAGE_KEY = "Email_Forgot_Password_Message";
        private const string EMAIL_FORGOT_PASSWORD_MESSAGE_USERNAME_KEY = "Email_Forgot_Password_Message_Username";

        private readonly IEmailResourceProvider _emailResourceProvider;

        public DefaultForgotPasswordEmailHtmlMessageProvider(IEmailResourceProvider emailResourceProvider)
        {
            _emailResourceProvider = emailResourceProvider;
        }

        public Task<KeyValuePair<string, string>> GetSubjectAndHtmlMessage(string userName, string callbackUrl)
        {
            var hasUserName = !string.IsNullOrEmpty(userName);

            return Task.FromResult(
                new KeyValuePair<string, string>
                (
                    _emailResourceProvider.GetStringOrDefault(EMAIL_FORGOT_PASSWORD_SUBJECT_KEY, "Forgot Password"),
                    hasUserName
                        ? string.Format(_emailResourceProvider.GetStringOrDefault(
                            EMAIL_FORGOT_PASSWORD_MESSAGE_USERNAME_KEY, "Hi, {0}! <br/> Please reset your password by <a href='{1}'>clicking here</a>."), userName, HtmlEncoder.Default.Encode(callbackUrl))
                        : string.Format(_emailResourceProvider.GetStringOrDefault(
                            EMAIL_FORGOT_PASSWORD_MESSAGE_KEY, "Please reset your password by <a href='{0}'>clicking here</a>."), HtmlEncoder.Default.Encode(callbackUrl)))
                );
        }
    }
}
