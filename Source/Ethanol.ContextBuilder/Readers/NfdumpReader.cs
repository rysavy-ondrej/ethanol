using CsvHelper;
using Ethanol.ContextBuilder.Cleaners;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Pipeline;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Readers.DataObjects;
using System;
using System.Globalization;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Reads export from nfdump tool, which is CSV with a large number of columns, from which only 
    /// their subset is relevant to the reader.
    /// </summary>
    [Plugin(PluginType.Reader, "NfdumpCsv", "Reads CSV file produced by nfdump.")]
    class NfdumpReaderPlugin : IFlowReader<IpFlow>
    {
        readonly Subject<IpFlow> _subject = new Subject<IpFlow>();
        private readonly NfdumpReader _dumpReader;
        private readonly FlowPairing _cleaner;

        public PipelineNodeType NodeType => PipelineNodeType.Producer;

        public NfdumpReaderPlugin(TextReader reader)
        {
            _dumpReader = new NfdumpReader(reader);
            _cleaner = new FlowPairing(TimeSpan.FromMinutes(3));

            _dumpReader.Subscribe(_cleaner);
            _cleaner.Subscribe(_subject);
        }

        public class Configuration
        {
            [YamlMember(Alias = "file", Description = "The file name with JSON data to read.")]
            public string FileName { get; set; }
        }



        /// <summary>
        /// Creates a new reader for the given arguments.
        /// </summary>
        /// <param name="arguments">Collection of arguments used to create a reader.</param>
        /// <returns>A new <see cref="FlowexpJsonReader"/> object.</returns>
        [PluginCreate]
        public static NfdumpReaderPlugin Create(Configuration configuration)
        {
            var reader = configuration.FileName != null ? File.OpenText(configuration.FileName) : System.Console.In;
            return new NfdumpReaderPlugin(reader);
        }

        public IDisposable Subscribe(IObserver<IpFlow> observer)
        {
            return _subject.Subscribe(observer); 
        }

        public Task StartReading()
        {
            return _dumpReader.StartReading();
        }
    }
    class NfdumpReader : FlowReader<IpFlow>
    {
            private readonly CsvReader _csvReader;

            /// <summary>
            /// Initializes the reader with underlying <see cref="StreamReader"/>.
            /// </summary>
            /// <param name="reader">The text reader device (input file or standard input).</param>
    public NfdumpReader(TextReader reader)
        {
            _csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

        }

        /// <summary>
        /// Provides next record form the input or null.
        /// </summary>
        /// <param name="ipfixRecord">The record that was read or null.</param>
        /// <returns>true if recrod was read or null for EOF reached.</returns>
        public bool TryReadNextEntry(out IpFlow ipfixRecord)
        {
            ipfixRecord = null;
            if (_csvReader.Read())
            {
                var record = _csvReader.GetRecord<NfdumpEntry>();
                if (record == null) return false;
                ipfixRecord = record.ToFlow();
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void Open()
        {
            _csvReader.Read();
            _csvReader.ReadHeader();
        }
        /// <inheritdoc/>
        protected override bool TryGetNextRecord(CancellationToken ct, out IpFlow record)
        {
            return TryReadNextEntry(out record);
        }
        /// <inheritdoc/>
        protected override void Close()
        {
            _csvReader.Dispose();
        }
    }
}
