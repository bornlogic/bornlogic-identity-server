namespace Bornlogic.IdentityServer.Services
{
    /// <summary>
    /// Models making HTTP requests for back-channel logout notification.
    /// </summary>
    public interface IBackChannelLogoutHttpClient
    {
        /// <summary>
        /// Performs HTTP POST.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        Task PostAsync(string url, Dictionary<string, string> payload);
    }
}