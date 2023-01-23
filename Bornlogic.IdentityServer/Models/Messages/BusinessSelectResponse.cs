using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bornlogic.IdentityServer.Models.Messages.Enums;

namespace Bornlogic.IdentityServer.Models.Messages
{
    public class BusinessSelectResponse
    {
        public AuthorizationError? Error { get; set; }
        public string BusinessId { get; set; }
        public string SubjectBusinessScopedId { get; set; }
    }

}
