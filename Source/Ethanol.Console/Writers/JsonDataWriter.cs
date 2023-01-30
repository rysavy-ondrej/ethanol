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
        public static JsonDataWriter Create(IReadOnlyDictionary<string, string> arguments)
        {
            var writer = arguments.TryGetValue("file", out var inputFile) ? File.CreateText(inputFile) : System.Console.Out;
            return new JsonDataWriter(writer);
        }
        public JsonDataWriter(TextWriter writer)
        {
            this._writer = writer;
            this._jsonOptions = new JsonSerializerOptions { };
        }

        protected override void Close()
        {
            _writer.Close();
        }

        protected override void Open()
        {
            // already opened.    
        }

        
        protected override void Write(object value)
        {
            _writer.WriteLine(JsonSerializer.Serialize(value));
        }
    }
}
