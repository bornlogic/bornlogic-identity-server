using Bornlogic.IdentityServer.Events.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Services.Default
{
    /// <summary>
    /// Default implementation of the event service. Write events raised to the log.
    /// </summary>
    public class DefaultEventSink : IEventSink
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEventSink"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DefaultEventSink(ILogger<DefaultEventService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Raises the specified event.
        /// </summary>
        /// <param name="evt">The event.</param>
        /// <exception cref="System.ArgumentNullException">evt</exception>
        public virtual Task PersistAsync(Event evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));

            _logger.LogInformation("{@event}", evt);

            return Task.CompletedTask;
        }
    }
}