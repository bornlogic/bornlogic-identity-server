using Bornlogic.IdentityServer.Infrastructure;
using Bornlogic.IdentityServer.Models.Messages;

namespace Bornlogic.IdentityServer.Stores.Default
{
    internal class BusinessSelectMessageStore : IBusinessSelectMessageStore
    {
        protected readonly MessageCookie<BusinessSelectResponse> Cookie;

        public BusinessSelectMessageStore(MessageCookie<BusinessSelectResponse> cookie)
        {
            Cookie = cookie;
        }

        public virtual Task DeleteAsync(string id)
        {
            Cookie.Clear(id);
            return Task.CompletedTask;
        }

        public virtual Task<Message<BusinessSelectResponse>> ReadAsync(string id)
        {
            return Task.FromResult(Cookie.Read(id));
        }

        public virtual Task WriteAsync(string id, Message<BusinessSelectResponse> message)
        {
            Cookie.Write(id, message);
            return Task.CompletedTask;
        }
    }
}
