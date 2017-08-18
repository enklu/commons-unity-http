using System;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// Provides an interface for serialization.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes an object into bytes.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="bytes">The byte output.</param>
        /// <exception cref="NullReferenceException"></exception>
        void Serialize(object value, out byte[] bytes);

        /// <summary>
        /// Deserializes an object.
        /// </summary>
        /// <param name="type">The type to deserialize into.</param>
        /// <param name="bytes">The input bytes to deserialize.</param>
        /// <param name="value">The resulting deserialized value.</param>
        /// <exception cref="NullReferenceException"></exception>
        void Deserialize(Type type, ref byte[] bytes, out object value);
    }
}