using Ethanol.ContextBuilder.Plugins.Attributes;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Produces NDJSON output for arbitrary object type.
    /// </summary>
    [Plugin(PluginType.Writer, "JsonWriter", "Writes NDJSON formatted file for computed context.")]
    public class JsonDataWriter : ContextWriter<object>
    {
        private readonly TextWriter _writer;
        JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Creates a new <see cref="JsonDataWriter"/> object. 
        /// <para/>
        /// Arguments:
        /// <para/>
        /// file=OUTPUT_FILE specifies that the output will be written to OUTPUT_FILE instead to standard output.
        /// </summary>
        /// <param name="arguments">The arguments used in object creation.</param>
        /// <returns>The new  <see cref="JsonDataWriter"/> object. </returns>
        [PluginCreate]
        public static JsonDataWriter Create(Configuration configuration)
        {
            var writer = configuration.FileName != null ? File.CreateText(configuration.FileName) : System.Console.Out;
            return new JsonDataWriter(writer);
        }

        public class Configuration
        {
            [PluginParameter(Name: "file", PluginParameterFlag.Optional, Description: "The file name with YAML data to write.")]
            [YamlMember(Alias = "file")]
            public string FileName { get; set; }
        }
        /// <summary>
        /// Creates a JSON writer for the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The text writer to produce the output.</param>
        public JsonDataWriter(TextWriter writer)
        {
            _writer = writer;
            _jsonOptions = new JsonSerializerOptions { };
            _jsonOptions.AddIPAddressConverter();

        }
        /// <inheritdoc/>
        protected override void Close()
        {
            _writer.Close();
        }
        /// <inheritdoc/>
        protected override void Open()
        {
            // already opened.    
        }
        /// <inheritdoc/>
        protected override void Write(object value)
        {
            _writer.WriteLine(JsonSerializer.Serialize(value, _jsonOptions));
        }

        public override string ToString()
        {
            return $"{nameof(JsonDataWriter)} (Writer={_writer})";
        }
    }
    public class IPAddressConverter : JsonConverter<IPAddress>
    {
        public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string ipAddressString = reader.GetString();
            return IPAddress.Parse(ipAddressString);
        }

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
    public static class JsonSerializerOptionsExtensions
    {
        public static void AddIPAddressConverter(this JsonSerializerOptions options)
        {
            options.Converters.Add(new IPAddressConverter());
        }
    }
}
