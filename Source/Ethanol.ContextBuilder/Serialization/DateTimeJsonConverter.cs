using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ethanol.ContextBuilder.Serialization
{

    /// <summary>
    /// Provides a custom JSON converter for <see cref="DateTime"/> type. This class facilitates custom 
    /// serialization and deserialization logic for DateTime objects when using the System.Text.Json library.
    /// </summary>
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        /// <summary>
        /// Reads and converts the DateTime from its JSON representation.
        /// </summary>
        /// <param name="reader">The UTF-8 JSON reader.</param>
        /// <param name="typeToConvert">The type to convert. Expected to be <see cref="DateTime"/>.</param>
        /// <param name="options">Options for the serializer.</param>
        /// <returns>The deserialized DateTime value.</returns>
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime));
            var datetime = DateTime.Parse(reader.GetString() ?? string.Empty);
            return datetime;
        }

        /// <summary>
        /// Writes a DateTime value as a JSON string using the specified writer and options.
        /// </summary>
        /// <param name="writer">The UTF-8 JSON writer to which the DateTime is to be written.</param>
        /// <param name="value">The DateTime value to write.</param>
        /// <param name="options">Options for the serializer.</param>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

}



