using AutoMapper;
using Ethanol.Console.DataObjects;
using System.IO;

namespace Ethanol.Console.Readers
{
    class IpfixcolEntryReader : IIpfixRecordReader
    {
        private readonly TextReader textReader;
        private readonly MapperConfiguration configuration;
        private readonly IMapper mapper;

        public IpfixcolEntryReader(TextReader textReader)
        {
            this.textReader = textReader;
            configuration = new MapperConfiguration(cfg =>
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
            mapper = configuration.CreateMapper();
        }

        public bool TryReadNextEntry(out IpfixRecord ipfixRecord)
        {
            ipfixRecord = null;
            var line = textReader.ReadLine();
            if (line == null)
            {
                return false;
            }
            if (IpfixcolEntry.TryDeserialize(line, out var entry))
            {
                ipfixRecord = mapper.Map<IpfixRecord>(entry);
                return true;
            }
            return false;
        }
    }
}
