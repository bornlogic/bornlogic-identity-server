namespace Bornlogic.IdentityServer.Email.HtmlMessageProvider.Contracts
{
    public interface IForgotPasswordEmailHtmlMessageProvider
    {
        Task<KeyValuePair<string, string>> GetSubjectAndHtmlMessage(string userName, string callbackUrl);
    }
}
