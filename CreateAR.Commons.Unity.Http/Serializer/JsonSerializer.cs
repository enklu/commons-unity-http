using System;
using System.Text;
using Newtonsoft.Json;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// Json implementation of ISerializer.
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        /// <inheritdoc cref="ISerializer"/>
        public void Serialize(object value, out byte[] bytes)
        {
            bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
        }

        /// <inheritdoc cref="ISerializer"/>
        public void Deserialize(Type type, ref byte[] bytes, out object value)
        {
            value = JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(bytes),
                type);
        }
    }
}