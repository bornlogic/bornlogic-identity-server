using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.Services.Default
{
    public class DefaultPendingTosService : IPendingTosService
    {
        public Task<bool> HasPendingTos(ClaimsPrincipal subject, Client client)
        {
            return Task.FromResult(false);
        }
    }
}
