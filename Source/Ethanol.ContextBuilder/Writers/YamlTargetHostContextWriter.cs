using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Polishers;
using Ethanol.ContextBuilder.Serialization;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Represents a writer capable of serializing data into the YAML format.
    /// </summary>
    /// <remarks>
    /// The <see cref="YamlTargetHostContextWriter"/> class is designed to take in specific data contexts (in this case, <see cref="IpTargetHostContext"/>) and 
    /// serialize them into the YAML (YAML Ain't Markup Language) format. 
    /// 
    /// Adorned with the <see cref="Plugin"/> attribute, this class is recognized as a plugin within the system, specifically categorized as a writer.
    /// The plugin's name is "YamlWriter", and its description suggests its primary role: to write or produce YAML formatted files based on computed contexts.
    /// 
    /// By inheriting from the <see cref="ContextWriter{T}"/> base class, the `YamlDataWriter` ensures it adheres to a standard interface or pattern for 
    /// writing contexts, but with specialization for the YAML format.
    /// </remarks>
    [Plugin(PluginCategory.Writer, "YamlWriter", "Writes YAML formatted file for computed context.")]
    public class YamlTargetHostContextWriter : ContextWriter<ObservableEvent<IpTargetHostContext>>
    {
        private readonly TextWriter _writer;
        private readonly ISerializer _yamlSerializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).WithTypeConverter(new IPAddressYamlTypeConverter()).DisableAliases().Build();

        /// <summary>
        /// Creates a new <see cref="YamlTargetHostContextWriter"/> object. 
        /// <para/>
        /// Arguments:
        /// <para/>
        /// file=OUTPUT_FILE specifies that the output will be written to OUTPUT_FILE instead to standard output.
        /// </summary>
        /// <param name="configuration">The configuration used in object creation.</param>
        /// <returns>The new  <see cref="YamlTargetHostContextWriter"/> object. </returns>
        /// 
        [PluginCreate]
        public static YamlTargetHostContextWriter Create(Configuration configuration)
        {
            var writer = configuration.FileName != null ? File.CreateText(configuration.FileName) : System.Console.Out;
            return new YamlTargetHostContextWriter(writer);
        }
        /// <summary>
        /// Creates a YAML writer for the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The text writer to produce the output.</param>
        public YamlTargetHostContextWriter(TextWriter writer)
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
        protected override void Write(ObservableEvent<IpTargetHostContext> value)
        {
            _writer.Write(_yamlSerializer.Serialize(value));
            _writer.WriteLine("---");
        }

        /// <summary>
        /// Represents the configuration settings for a writer.
        /// </summary>
        /// <remarks>
        /// The <see cref="Configuration"/> class encapsulates settings that can be used to customize or configure operations related to YAML data writing. 
        /// </remarks>
        public class Configuration
        {
            /// <summary>
            /// Gets or sets the file name for the YAML data to be written.
            /// </summary>
            [PluginParameter(Name: "file", PluginParameterFlag.Optional, Description: "The file name with YAML data to write.")]
            [YamlMember(Alias = "file")]
            public string FileName { get; set; }
        }

    }
}
