using Bornlogic.IdentityServer.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bornlogic.IdentityServer.Stores
{
    public interface IAcceptTosMessageStore
    {
        /// <summary>
        /// Writes the business select response message.
        /// </summary>
        /// <param name="id">The id for the message.</param>
        /// <param name="message">The message.</param>
        Task WriteAsync(string id, Message<AcceptTosResponse> message);

        /// <summary>
        /// Reads the business select response message.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<Message<AcceptTosResponse>> ReadAsync(string id);

        /// <summary>
        /// Deletes the business select response message.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task DeleteAsync(string id);
    }
}
