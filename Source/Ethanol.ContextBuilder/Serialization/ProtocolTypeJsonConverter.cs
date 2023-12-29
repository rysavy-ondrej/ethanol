using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ethanol.ContextBuilder.Serialization
{
    /// <summary>
    /// Converts a <see cref="ProtocolType"/> to and from JSON.
    /// </summary>
    public class ProtocolTypeJsonConverter : JsonConverter<ProtocolType>
        {
            /// <summary>
            /// Reads a JSON value and converts it to a <see cref="ProtocolType"/>.
            /// </summary>
            /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
            /// <param name="typeToConvert">The type of the object to convert.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for deserialization.</param>
            /// <returns>The deserialized <see cref="ProtocolType"/>.</returns>
            public override ProtocolType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                Debug.Assert(typeToConvert == typeof(ProtocolType));

                switch(reader.TokenType)
                {
                    case JsonTokenType.Number:
                        if (reader.TryGetInt16(out short protocolTypeValue))
                        {
                            return (ProtocolType)protocolTypeValue;
                        }
                        throw new JsonException($"Unable to convert Number to {nameof(ProtocolType)}");

                    case JsonTokenType.String:
                        var stringValue = reader.GetString();
                        
                        if (stringValue == null) throw new JsonException($"Unable to convert Null to {nameof(ProtocolType)}");

                        if (stringValue.StartsWith("IPv6-", StringComparison.OrdinalIgnoreCase))
                        {
                            stringValue = stringValue.Substring(5);
                        }

                        if (Enum.TryParse<ProtocolType>(stringValue, true, out var protocolType))
                        {
                            return protocolType;
                        }
                        if (Int16.TryParse(stringValue, out protocolTypeValue))
                        {
                            return (ProtocolType)protocolTypeValue;
                        }
                        throw new JsonException($"Unable to convert String '{stringValue}' to {nameof(ProtocolType)}");
                }
                throw new JsonException($"Unable to convert {reader.TokenType} to {nameof(ProtocolType)}");
            }

            /// <summary>
            /// Writes a <see cref="ProtocolType"/> value as JSON.
            /// </summary>
            /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
            /// <param name="value">The <see cref="ProtocolType"/> value to write.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for serialization.</param>
            public override void Write(Utf8JsonWriter writer, ProtocolType value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

}



