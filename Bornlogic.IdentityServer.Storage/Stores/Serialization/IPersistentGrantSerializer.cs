﻿namespace Bornlogic.IdentityServer.Storage.Stores.Serialization
{
    /// <summary>
    /// Interface for persisted grant serialization
    /// </summary>
    public interface IPersistentGrantSerializer
    {
        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        string Serialize<T>(T value);

        /// <summary>
        /// Deserializes the specified string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        T Deserialize<T>(string json);
    }
}