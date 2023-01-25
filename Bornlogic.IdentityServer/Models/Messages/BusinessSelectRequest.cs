using IdentityModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bornlogic.IdentityServer.Models.Messages
{
    public class BusinessSelectRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessSelectRequest"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="subject">The subject.</param>
        public BusinessSelectRequest(AuthorizationRequest request, string subject)
        {
            ClientId = request.Client.ClientId;
            Nonce = request.Parameters[OidcConstants.AuthorizeRequest.Nonce];
            Subject = subject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessSelectRequest"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="subject">The subject.</param>
        public BusinessSelectRequest(NameValueCollection parameters, string subject)
        {
            ClientId = parameters[OidcConstants.AuthorizeRequest.ClientId];
            Nonce = parameters[OidcConstants.AuthorizeRequest.Nonce];
            Subject = subject;
        }

        public BusinessSelectRequest(string clientId, string nonce, string subject)
        {
            ClientId = clientId;
            Nonce = nonce;
            Subject = subject;
        }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        /// <value>
        /// The nonce.
        /// </value>
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id
        {
            get
            {
                var value = $"{ClientId}:{Subject}:{Nonce}";

                using (var sha = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(value);
                    var hash = sha.ComputeHash(bytes);

                    return Base64Url.Encode(hash);
                }
            }
        }
    }
}
