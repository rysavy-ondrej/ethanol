using AutoMapper;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Readers.DataObjects;
using System.IO;
using System.Threading;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Reads <see cref="IpFlow"/> collection as exported from Ipfixcol2 tool (https://github.com/CESNET/ipfixcol2), which is basically NDJSON.
    /// </summary>
    [Plugin(PluginType.Reader, "IpfixcolJson", "Reads NDJSON exported from ipfixcol2 tool.")]
    public class IpfixcolReader : FlowReader<IpFlow>
    {
        private readonly TextReader _textReader;

        /// <summary>
        /// Creates a new reader for the given arguments.
        /// </summary>
        /// <param name="arguments">Collection of arguments used to create a reader.</param>
        /// <returns>A new <see cref="IpfixcolReader"/> object.</returns>
        [PluginCreate]
        public static IpfixcolReader Create(Configuration configuration)
        {
            var reader = configuration.FileName != null ? File.OpenText(configuration.FileName) : System.Console.In;
            return new IpfixcolReader(reader);
        }
        /// <summary>
        /// Creates a reader from the underlying text reader.
        /// </summary>
        /// <param name="textReader">The text reader device used to read data from.</param>
        public IpfixcolReader(TextReader textReader)
        {
            this._textReader = textReader;

        }

        /// <inheritdoc/>
        protected override void Open()
        {

        }
        /// <inheritdoc/>
        protected override bool TryGetNextRecord(CancellationToken ct, out IpFlow ipFlow)
        {
            ipFlow = null;
            var line = _textReader.ReadLine();
            if (line == null)
            {
                return false;
            }
            if (IpfixcolEntry.TryDeserialize(line, out var entry))
            {
                ipFlow = entry.ToFlow();
            }
            return false;
        }
        /// <inheritdoc/>
        protected override void Close()
        {
            _textReader.Close();
        }
        public class Configuration
        {
            [YamlMember(Alias = "file", Description = "The file name with JSON data to read.")]
            public string FileName { get; set; }
        }
    }
}
