using Ethanol.ContextBuilder.Plugins.Attributes;
using System.IO;
using System.Text.Json;
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
        public static JsonDataWriter Create(Configuration configuration)
        {
            var writer = configuration.FileName != null ? File.CreateText(configuration.FileName) : System.Console.Out;
            return new JsonDataWriter(writer);
        }

        public class Configuration
        {
            [YamlMember(Alias = "file", Description = "The file name with NDJSON data to write.")]
            public string FileName { get; set; }
        }
        /// <summary>
        /// Creates a JSON writer for the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The text writer to produce the output.</param>
        public JsonDataWriter(TextWriter writer)
        {
            this._writer = writer;
            this._jsonOptions = new JsonSerializerOptions { };
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
            _writer.WriteLine(JsonSerializer.Serialize(value));
        }
    }
}
