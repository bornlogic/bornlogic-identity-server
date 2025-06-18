using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bornlogic.IdentityServer.Logging
{
    /// <summary>
    /// Helper to JSON serialize object data for logging.
    /// </summary>
    internal static class LogSerializer
    {
        static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        static LogSerializer()
        {
            Options.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="logObject">The object.</param>
        /// <returns></returns>
        public static string Serialize(object logObject)
        {
            return JsonSerializer.Serialize(logObject, Options);
        }
    }
}