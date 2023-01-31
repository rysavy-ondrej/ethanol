using AutoMapper;
using Ethanol.ContextBuilder;
using Ethanol.ContextBuilder.DataObjects;
using Ethanol.ContextBuilder.Context;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Reads <see cref="IpfixRecord"/> collection as exported from Ipfixcol2 tool (https://github.com/CESNET/ipfixcol2), which is basically NDJSON.
    /// </summary>
    class IpfixcolEntryReader : InputDataReader<IpfixRecord>
    {
        private readonly TextReader _textReader;
        private readonly MapperConfiguration _configuration;
        private readonly IMapper _mapper;


        /// <summary>
        /// Creates a new reader for the given arguments.
        /// </summary>
        /// <param name="arguments">Collection of arguments used to create a reader.</param>
        /// <returns>A new <see cref="IpfixcolEntryReader"/> object.</returns>
        public static IpfixcolEntryReader Create(IReadOnlyDictionary<string, string> arguments)
        {
            var reader = arguments.TryGetValue("file", out var inputFile) ? File.OpenText(inputFile) : System.Console.In;
            arguments.TryGetValue("format", out var format);
            return new IpfixcolEntryReader(reader);
        }
        /// <summary>
        /// Creates a reader from the underlying text reader.
        /// </summary>
        /// <param name="textReader">The text reader device used to read data from.</param>
        public IpfixcolEntryReader(TextReader textReader)
        {
            this._textReader = textReader;
            _configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IpfixcolEntry, IpfixRecord>()
                .ForMember(d => d.Bytes, o => o.MapFrom(s => s.IanaOctetDeltaCount))
                .ForMember(d => d.DestinationIpAddress, o => o.MapFrom(s => s.IanaDestinationIPv4Address))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.IanaDestinationTransportPort))
                .ForMember(d => d.DnsQueryName, o => o.MapFrom(s => s.FlowmonDnsQname.Replace("\0", "")))
                .ForMember(d => d.DnsResponseData, o => o.MapFrom(s => s.FlowmonDnsCrrRdata.Replace("\0", "")))
                .ForMember(d => d.HttpHost, o => o.MapFrom(s => s.FlowmonHttpHost.Replace("\0", "")))
                .ForMember(d => d.Packets, o => o.MapFrom(s => s.IanaPacketDeltaCount))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => s.IanaProtocolIdentifier))
                .ForMember(d => d.SourceIpAddress, o => o.MapFrom(s => s.IanaSourceIPv4Address))
                .ForMember(d => d.SourceTransportPort, o => o.MapFrom(s => s.IanaSourceTransportPort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.IanaFlowStartMilliseconds))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.IanaFlowEndMilliseconds - s.IanaFlowStartMilliseconds))
                .ForMember(d => d.TlsClientVersion, o => o.MapFrom(s => s.FlowmonTlsClientVersion))
                .ForMember(d => d.TlsJa3, o => o.MapFrom(s => s.FlowmonTlsJa3Fingerprint))
                .ForMember(d => d.TlsServerCommonName, o => o.MapFrom(s => s.FlowmonTlsSubjectCn.Replace("\0", "")))
                .ForMember(d => d.TlsServerName, o => o.MapFrom(s => s.FlowmonTlsSni.Replace("\0", "")));
            });
            _mapper = _configuration.CreateMapper();
        }

        /// <summary>
        /// Provides next record form the input or null.
        /// </summary>
        /// <param name="ipfixRecord">The record that was read or null.</param>
        /// <returns>true if recrod was read or null for EOF reached.</returns>
        public bool TryReadNextEntry(out IpfixRecord ipfixRecord)
        {
            ipfixRecord = null;
            var line = _textReader.ReadLine();
            if (line == null)
            {
                return false;
            }
            if (IpfixcolEntry.TryDeserialize(line, out var entry))
            {
                ipfixRecord = _mapper.Map<IpfixRecord>(entry);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void Open()
        {

        }
        /// <inheritdoc/>
        protected override bool TryGetNextRecord(CancellationToken ct, out IpfixRecord record)
        {
            return TryReadNextEntry(out record);
        }
        /// <inheritdoc/>
        protected override void Close()
        {
            _textReader.Close();
        }
    }
}
