using Newtonsoft.Json;

namespace Bornlogic.IdentityServer.Storage.Stores.Serialization
{
    /// <summary>
    /// JSON-based persisted grant serializer
    /// </summary>
    /// <seealso cref="IPersistentGrantSerializer" />
    public class PersistentGrantSerializer : IPersistentGrantSerializer
    {
        private static readonly JsonSerializerSettings _settings;

        static PersistentGrantSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                ContractResolver = new CustomContractResolver()
            };
            _settings.Converters.Add(new ClaimConverter());
            _settings.Converters.Add(new ClaimsPrincipalConverter());
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _settings);
        }

        /// <summary>
        /// Deserializes the specified string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
}