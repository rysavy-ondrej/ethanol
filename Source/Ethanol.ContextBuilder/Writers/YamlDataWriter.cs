using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// This object can write the data in the YAML format. 
    /// </summary>
    public class YamlDataWriter : WriterModule<object>
    {
        private readonly TextWriter _writer;
        private readonly ISerializer _yamlSerializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases().Build();

        /// <summary>
        /// Creates a new <see cref="YamlDataWriter"/> object. 
        /// <para/>
        /// Arguments:
        /// <para/>
        /// file=OUTPUT_FILE specifies that the output will be written to OUTPUT_FILE instead to standard output.
        /// </summary>
        /// <param name="arguments">The arguments used in object creation.</param>
        /// <returns>The new  <see cref="YamlDataWriter"/> object. </returns>
        public static YamlDataWriter Create(IReadOnlyDictionary<string, string> arguments)
        {
            var writer = arguments.TryGetValue("file", out var inputFile) ? File.CreateText(inputFile) : System.Console.Out;
            return new YamlDataWriter(writer);
        }
        /// <summary>
        /// Creates a YAML writer for the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The text writer to produce the output.</param>
        public YamlDataWriter(TextWriter writer)
        {
            this._writer = writer;
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
            _writer.Write(_yamlSerializer.Serialize(value));
            _writer.WriteLine("---");
        }
    }
}
