using Bornlogic.IdentityServer.Infrastructure;
using Bornlogic.IdentityServer.Models.Messages;

namespace Bornlogic.IdentityServer.Stores.Default
{
    internal class AcceptTosMessageStore : IAcceptTosMessageStore
    {
        protected readonly MessageCookie<AcceptTosResponse> Cookie;

        public AcceptTosMessageStore(MessageCookie<AcceptTosResponse> cookie)
        {
            Cookie = cookie;
        }

        public virtual Task DeleteAsync(string id)
        {
            Cookie.Clear(id);
            return Task.CompletedTask;
        }

        public virtual Task<Message<AcceptTosResponse>> ReadAsync(string id)
        {
            return Task.FromResult(Cookie.Read(id));
        }

        public virtual Task WriteAsync(string id, Message<AcceptTosResponse> message)
        {
            Cookie.Write(id, message);
            return Task.CompletedTask;
        }
    }
}
