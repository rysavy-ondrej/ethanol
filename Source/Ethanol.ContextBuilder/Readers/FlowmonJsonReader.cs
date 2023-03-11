using AutoMapper;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Readers.DataObjects;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Reads export from flowmonexp5 in JSON format. This format is specific by 
    /// representing each flow as an individual JSON object. It is not NDJSON nor 
    /// properly formatted array of JSON objects.
    /// </summary>
    [Plugin(PluginType.Reader, "FlowmonJson", "Reads JSON file with IPFIX data produced by flowmonexp5 tool.")]
    class FlowexpJsonReader : FlowReader<IpFlow>
    {
        private readonly TextReader _reader;
        private readonly JsonSerializerOptions _serializerOptions;


        public class Configuration
        {
            [YamlMember(Alias = "file", Description = "The file name with JSON data to read. If not provided then STDIN will be used.")]
            public string FileName { get; set; }
        }

        /// <summary>
        /// Creates a new reader for the given arguments.
        /// </summary>
        /// <param name="arguments">Collection of arguments used to create a reader.</param>
        /// <returns>A new <see cref="FlowexpJsonReader"/> object.</returns>
        [PluginCreate]
        public static FlowexpJsonReader Create(Configuration configuration)
        {
            var reader = configuration.FileName != null ? File.OpenText(configuration.FileName) : System.Console.In;
            return new FlowexpJsonReader(reader);
        }

        /// <summary>
        /// Initializes the reader with underlying <see cref="TextReader"/>.
        /// </summary>
        /// <param name="reader">The text reader device (input file or standard input).</param>
        public FlowexpJsonReader(TextReader reader)
        {
            _reader = reader;
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new DateTimeJsonConverter());
        }


        /// <summary>
        /// Reads input record from Flowmon's specific JSON or NDJSON.
        /// </summary>
        private string ReadJsonString(TextReader inputStream)
        {
            var buffer = new StringBuilder();
            while (true)
            {
                var line = inputStream.ReadLine();

                // End of file?
                if (line == null) break;

                buffer.AppendLine(line);

                // Do we have NDJSON?
                if (line.StartsWith("{") && line.EndsWith("}")) break;

                // end of multiline JSON object?
                if (line.Trim() == "}") break;
            }
            var record = buffer.ToString().Trim();
            if (string.IsNullOrWhiteSpace(record))
            {
                return null;
            }
            else
            {
                return record;
            }
        }

        /// <inheritdoc/>
        protected override void Open()
        {

        }

        /// <inheritdoc/>
        protected override bool TryGetNextRecord(CancellationToken ct, out IpFlow ipFlow)
        {
            ipFlow = null;
            var line = ReadJsonString(_reader);
            if (line == null) return false;
            if (TryDeserialize(line, out var _currentEntry))
            {
                ipFlow = _currentEntry.ToFlow();
                return true;
            }
            return false;
        }
        /// <inheritdoc/>
        protected override void Close()
        {
            _reader.Close();
        }
        bool TryDeserialize(string input, out FlowexpEntry entry)
        {
            try
            {
                entry = JsonSerializer.Deserialize<FlowexpEntry>(input, _serializerOptions);
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Cannot deserialize entry: {e.Message}");
                entry = default;
                return false;
            }
        }

        public override string ToString()
        {
            return $"{nameof(FlowexpJsonReader)}(Reader={_reader})"; 
        }
    }
}
