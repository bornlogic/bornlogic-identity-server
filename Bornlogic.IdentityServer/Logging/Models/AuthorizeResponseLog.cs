using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.ResponseHandling.Models;

namespace Bornlogic.IdentityServer.Logging.Models
{
    internal class AuthorizeResponseLog
    {
        public string SubjectId { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string State { get; set; }

        public string Scope { get; set; }
        public string Error { get; set; }
        public string SubError { get; set; }


        public AuthorizeResponseLog(AuthorizeResponse response)
        {
            ClientId = response.Request?.Client?.ClientId;
            SubjectId = response.Request?.Subject?.GetSubjectId();
            RedirectUri = response.RedirectUri;
            State = response.State;
            Scope = response.Scope;
            Error = response.Error;
            SubError = response.SubError;
        }

        public override string ToString()
        {
            return LogSerializer.Serialize(this);
        }
    }
}