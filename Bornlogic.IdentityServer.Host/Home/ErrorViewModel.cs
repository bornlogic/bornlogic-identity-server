using Bornlogic.IdentityServer.Models.Messages;

namespace Bornlogic.IdentityServer.Host.Home
{
    public class ErrorViewModel
    {
        public ErrorViewModel()
        {
        }

        public ErrorViewModel(string error)
        {
            Error = new ErrorMessage { Error = error };
        }

        public ErrorMessage Error { get; set; }
    }
}