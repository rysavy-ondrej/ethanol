using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ethanol.ContextBuilder.Serialization
{
    /// <summary>
    /// Provides custom JSON serialization and deserialization for the <see cref="IPAddress"/> type.
    /// </summary>
    /// <remarks>
    /// This converter allows for the proper reading and writing of <see cref="IPAddress"/> objects 
    /// when using the System.Text.Json serialization process. It ensures that IP addresses are serialized 
    /// as strings in the JSON format and can be deserialized back into <see cref="IPAddress"/> objects.
    /// </remarks>
    public class IPAddressJsonConverter : JsonConverter<IPAddress>
    {
        /// <summary>
        /// Reads and converts the JSON representation of an IP address to its <see cref="IPAddress"/> equivalent.
        /// </summary>
        /// <param name="reader">The JSON reader to read from.</param>
        /// <param name="typeToConvert">Type of the object to convert.</param>
        /// <param name="options">Options for the serializer.</param>
        /// <returns>The deserialized IP address.</returns>
        public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string ipAddressString = reader.GetString();
            return IPAddress.Parse(ipAddressString);
        }

        /// <summary>
        /// Writes an <see cref="IPAddress"/> value as a JSON string.
        /// </summary>
        /// <param name="writer">The JSON writer to write to.</param>
        /// <param name="value">The IP address to write.</param>
        /// <param name="options">Options for the serializer.</param>
        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

}
