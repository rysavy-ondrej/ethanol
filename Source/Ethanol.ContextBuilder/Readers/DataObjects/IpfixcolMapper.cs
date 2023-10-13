using AutoMapper;
using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    public static class IpfixcolMapper
    {
        private static readonly MapperConfiguration _configuration;
        private static readonly IMapper _mapper;
        /*
_configuration = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<IpfixcolEntry, IpFlow>()
    .ForMember(d => d., o => o.MapFrom(s => s.IanaOctetDeltaCount))
    .ForMember(d => d.DestinationIpAddress, o => o.MapFrom(s => s.IanaDestinationIPv4Address))
    .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.IanaDestinationTransportPort))
    .ForMember(d => d.DnsQueryName, o => o.MapFrom(s => s.FlowmonDnsQname.Replace("\0", "")))
    .ForMember(d => d.DnsResponseData, o => o.MapFrom(s => s.FlowmonDnsCrrRdata.Replace("\0", "")))
    .ForMember(d => d.HttpHost, o => o.MapFrom(s => s.FlowmonHttpHost.Replace("\0", "")))
    .ForMember(d => d.Packets, o => o.MapFrom(s => s.IanaPacketDeltaCount))
    .ForMember(d => d.ProtocolIdentifier, o => o.MapFrom(s => s.IanaProtocolIdentifier))
    .ForMember(d => d.SourceIpAddress, o => o.MapFrom(s => s.IanaSourceIPv4Address))
    .ForMember(d => d.SourceTransportPort, o => o.MapFrom(s => s.IanaSourceTransportPort))
    .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.IanaFlowStartMilliseconds))
    .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.IanaFlowEndMilliseconds - s.IanaFlowStartMilliseconds))
    .ForMember(d => d.TlsVersion, o => o.MapFrom(s => s.FlowmonTlsClientVersion))
    .ForMember(d => d.TlsJa3, o => o.MapFrom(s => s.FlowmonTlsJa3Fingerprint))
    .ForMember(d => d.TlsSubjectCommonName, o => o.MapFrom(s => s.FlowmonTlsSubjectCn.Replace("\0", "")))
    .ForMember(d => d.TlsServerNameIndication, o => o.MapFrom(s => s.FlowmonTlsSni.Replace("\0", "")));
});
_mapper = _configuration.CreateMapper();
*/
        public static IpFlow ToFlow(this IpfixcolEntry flow)
        {
            return null;
        }
    }
}



