using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.Services.Default
{
    public class DefaultIntrospectionResponseEnricher : IIntrospectionResponseEnricher
    {
        public Task Enrich(IDictionary<string, object> response, IntrospectionRequestValidationResult validationResult)
        {
            return Task.CompletedTask;
        }
    }
}
