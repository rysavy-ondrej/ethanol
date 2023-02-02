using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using System.Text.Json.Serialization;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// This object can write the data in the YAML format. 
    /// </summary>
    [Plugin(PluginType.Writer, "YamlWriter", "Writes YAML formatted file for computed context.")]
    public class YamlDataWriter : ContextWriter<object>
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
        /// <param name="configuration">The configuration used in object creation.</param>
        /// <returns>The new  <see cref="YamlDataWriter"/> object. </returns>
        /// 
        [PluginCreate]
        public static YamlDataWriter Create(Configuration configuration)
        {
            var writer = configuration.FileName != null ? File.CreateText(configuration.FileName) : System.Console.Out;
            return new YamlDataWriter(writer);
        }
        public class Configuration
        {
            [PluginParameter(Name: "file", PluginParameterFlag.Optional, Description: "The file name with YAML data to write.")]
            [YamlMember(Alias="file")]
            public string FileName { get; set; }
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
