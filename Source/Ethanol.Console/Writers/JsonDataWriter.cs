using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Produces NDJSON output for arbitrary object type.
    /// </summary>
    public class JsonDataWriter : WriterModule<object>
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
        public static JsonDataWriter Create(IReadOnlyDictionary<string, string> arguments)
        {
            var writer = arguments.TryGetValue("file", out var inputFile) ? File.CreateText(inputFile) : System.Console.Out;
            return new JsonDataWriter(writer);
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
