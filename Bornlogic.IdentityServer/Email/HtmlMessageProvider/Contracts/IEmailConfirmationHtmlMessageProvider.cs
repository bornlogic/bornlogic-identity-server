namespace Bornlogic.IdentityServer.Email.HtmlMessageProvider.Contracts
{
    public interface IEmailConfirmationHtmlMessageProvider
    {
        Task<KeyValuePair<string, string>> GetSubjectAndHtmlMessage(string userName, string callbackUrl);
    }
}