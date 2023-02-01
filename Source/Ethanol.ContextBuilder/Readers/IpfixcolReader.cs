using AutoMapper;
using Ethanol.ContextBuilder;
using Ethanol.ContextBuilder.Context;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Ethanol.ContextBuilder.Readers.DataObjects;
using Ethanol.ContextBuilder.Attributes;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Reads <see cref="IpfixObject"/> collection as exported from Ipfixcol2 tool (https://github.com/CESNET/ipfixcol2), which is basically NDJSON.
    /// </summary>
    [Module(ModuleType.Reader, "IpfixcolJson")]
    class IpfixcolReader : ReaderModule<IpfixObject>
    {
        private readonly TextReader _textReader;
        private readonly MapperConfiguration _configuration;
        private readonly IMapper _mapper;


        /// <summary>
        /// Creates a new reader for the given arguments.
        /// </summary>
        /// <param name="arguments">Collection of arguments used to create a reader.</param>
        /// <returns>A new <see cref="IpfixcolReader"/> object.</returns>
        public static IpfixcolReader Create(IReadOnlyDictionary<string, string> arguments)
        {
            var reader = arguments.TryGetValue("file", out var inputFile) ? File.OpenText(inputFile) : System.Console.In;
            arguments.TryGetValue("format", out var format);
            return new IpfixcolReader(reader);
        }
        /// <summary>
        /// Creates a reader from the underlying text reader.
        /// </summary>
        /// <param name="textReader">The text reader device used to read data from.</param>
        public IpfixcolReader(TextReader textReader)
        {
            this._textReader = textReader;
            _configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IpfixcolEntry, IpfixObject>()
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
                .ForMember(d => d.TlsVersion, o => o.MapFrom(s => s.FlowmonTlsClientVersion))
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
        public bool TryReadNextEntry(out IpfixObject ipfixRecord)
        {
            ipfixRecord = null;
            var line = _textReader.ReadLine();
            if (line == null)
            {
                return false;
            }
            if (IpfixcolEntry.TryDeserialize(line, out var entry))
            {
                ipfixRecord = _mapper.Map<IpfixObject>(entry);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void Open()
        {

        }
        /// <inheritdoc/>
        protected override bool TryGetNextRecord(CancellationToken ct, out IpfixObject record)
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
