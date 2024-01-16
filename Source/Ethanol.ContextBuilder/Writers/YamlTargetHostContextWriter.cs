using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Serialization;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Collections.Generic;
using System;

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
    public class YamlTargetHostContextWriter : ContextWriter<HostContext>
    {
        private readonly TextWriter _writer;
        private readonly ISerializer _yamlSerializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).WithTypeConverter(new IPAddressYamlTypeConverter()).DisableAliases().Build();

        /// <summary>
        /// Creates a YAML writer for the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The text writer to produce the output.</param>
        public YamlTargetHostContextWriter(TextWriter writer)
        {
            this._writer = writer;
        }

        public override void OnWindowClosed(DateTimeOffset start, DateTimeOffset end)
        {
            _writer.Flush();
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
        protected override void Write(HostContext value)
        {
            _writer.Write(_yamlSerializer.Serialize(value));
            _writer.WriteLine("---");
        }

        protected override void WriteBatch(IEnumerable<HostContext> record)
        {
            foreach (var item in record)
            {
                Write(item);
            }
        }
    }
}
