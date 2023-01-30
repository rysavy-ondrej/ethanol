using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Writers
{
    public class YamlDataWriter : OutputDataWriter<object>
    {
        private readonly TextWriter _writer;
        private readonly ISerializer _yamlSerializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases().Build();

        public static YamlDataWriter Create(IReadOnlyDictionary<string, string> arguments)
        {
            var writer = arguments.TryGetValue("file", out var inputFile) ? File.CreateText(inputFile) : System.Console.Out;
            return new YamlDataWriter(writer);
        }
        public YamlDataWriter(TextWriter writer)
        {
            this._writer = writer;
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
            _writer.Write(_yamlSerializer.Serialize(value));
            _writer.WriteLine("---");
        }
    }
}
