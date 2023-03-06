using AutoMapper;
using Ethanol.ContextBuilder.Context;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// Representational object for JSON record created by ipfixcol2 tool. 
    /// <para/>
    /// The object is loaded from the output from ipfixcol2 tool. Use map to get corresponding IpfixRecord.
    /// </summary>
    public class IpfixcolEntry
    {
        public static bool TryDeserialize(string input, out IpfixcolEntry entry)
        {
            try
            {
                entry = JsonSerializer.Deserialize<IpfixcolEntry>(input);
                return true;
            }
            catch (Exception)
            {
                entry = default;
                return false;
            }
        }
        #region FLOWMON IPFIX Fields
        [JsonPropertyName("flowmon:dnsId")]
        public int FlowmonDnsId { get; set; }

        [JsonPropertyName("flowmon:dnsFlagsCodes")]
        public int FlowmonDnsFlagsCodes { get; set; }

        [JsonPropertyName("flowmon:dnsQuestionCount")]
        public int FlowmonDnsQuestionCount { get; set; }

        [JsonPropertyName("flowmon:dnsAnswrecCount")]
        public int FlowmonDnsAnswrecCount { get; set; }

        [JsonPropertyName("flowmon:dnsAuthrecCount")]
        public int FlowmonDnsAuthrecCount { get; set; }

        [JsonPropertyName("flowmon:dnsAddtrecCount")]
        public int FlowmonDnsAddtrecCount { get; set; }

        [JsonPropertyName("flowmon:dnsCrrName")]
        public string FlowmonDnsCrrName { get; set; }

        [JsonPropertyName("flowmon:dnsCrrType")]
        public int FlowmonDnsCrrType { get; set; }

        [JsonPropertyName("flowmon:dnsCrrClass")]
        public int FlowmonDnsCrrClass { get; set; }

        [JsonPropertyName("flowmon:dnsCrrTtl")]
        public int FlowmonDnsCrrTtl { get; set; }

        [JsonPropertyName("flowmon:dnsCrrRdata")]
        public string FlowmonDnsCrrRdata { get; set; }

        [JsonPropertyName("flowmon:dnsCrrRdataLen")]
        public int FlowmonDnsCrrRdataLen { get; set; }

        [JsonPropertyName("flowmon:dnsQname")]
        public string FlowmonDnsQname { get; set; }

        [JsonPropertyName("flowmon:dnsQtype")]
        public int FlowmonDnsQtype { get; set; }

        [JsonPropertyName("flowmon:dnsQclass")]
        public int FlowmonDnsQclass { get; set; }


        [JsonPropertyName("flowmon:httpHost")]
        public string FlowmonHttpHost { get; set; }

        [JsonPropertyName("flowmon:httpMethodMask")]
        public int FlowmonHttpMethodMask { get; set; }

        [JsonPropertyName("flowmon:tcpSynSize")]
        public int FlowmonTcpSynSize { get; set; }

        [JsonPropertyName("flowmon:tcpSynTtl")]
        public int FlowmonTcpSynTtl { get; set; }

        [JsonPropertyName("iana:tcpWindowSize")]
        public int IanaTcpWindowSize { get; set; }

        [JsonPropertyName("flowmon:tlsContentType")]
        public int FlowmonTlsContentType { get; set; }

        [JsonPropertyName("flowmon:tlsHandshakeType")]
        public int FlowmonTlsHandshakeType { get; set; }

        [JsonPropertyName("flowmon:tlsSetupTime")]
        public int FlowmonTlsSetupTime { get; set; }

        [JsonPropertyName("flowmon:tlsServerVersion")]
        public int FlowmonTlsServerVersion { get; set; }

        [JsonPropertyName("flowmon:tlsServerRandom")]
        public string FlowmonTlsServerRandom { get; set; }

        [JsonPropertyName("flowmon:tlsServerSessionId")]
        public string FlowmonTlsServerSessionId { get; set; }

        [JsonPropertyName("flowmon:tlsCipherSuite")]
        public int FlowmonTlsCipherSuite { get; set; }

        [JsonPropertyName("flowmon:tlsAlpn")]
        public string FlowmonTlsAlpn { get; set; }

        [JsonPropertyName("flowmon:tlsSni")]
        public string FlowmonTlsSni { get; set; }

        [JsonPropertyName("flowmon:tlsSniLength")]
        public int FlowmonTlsSniLength { get; set; }

        [JsonPropertyName("flowmon:tlsClientVersion")]
        public int FlowmonTlsClientVersion { get; set; }

        [JsonPropertyName("flowmon:tlsCipherSuites")]
        public string FlowmonTlsCipherSuites { get; set; }

        [JsonPropertyName("flowmon:tlsClientRandom")]
        public string FlowmonTlsClientRandom { get; set; }

        [JsonPropertyName("flowmon:tlsClientSessionId")]
        public string FlowmonTlsClientSessionId { get; set; }

        [JsonPropertyName("flowmon:tlsExtensionTypes")]
        public string FlowmonTlsExtensionTypes { get; set; }

        [JsonPropertyName("flowmon:tlsExtensionLengths")]
        public string FlowmonTlsExtensionLengths { get; set; }

        [JsonPropertyName("flowmon:tlsEllipticCurves")]
        public string FlowmonTlsEllipticCurves { get; set; }

        [JsonPropertyName("flowmon:tlsEcPointFormats")]
        public int FlowmonTlsEcPointFormats { get; set; }

        [JsonPropertyName("flowmon:tlsClientKeyLength")]
        public int FlowmonTlsClientKeyLength { get; set; }

        [JsonPropertyName("flowmon:tlsIssuerCn")]
        public string FlowmonTlsIssuerCn { get; set; }

        [JsonPropertyName("flowmon:tlsSubjectCn")]
        public string FlowmonTlsSubjectCn { get; set; }

        [JsonPropertyName("flowmon:tlsSubjectOn")]
        public string FlowmonTlsSubjectOn { get; set; }

        [JsonPropertyName("flowmon:tlsValidityNotBefore")]
        public int FlowmonTlsValidityNotBefore { get; set; }

        [JsonPropertyName("flowmon:tlsValidityNotAfter")]
        public int FlowmonTlsValidityNotAfter { get; set; }

        [JsonPropertyName("flowmon:tlsSignatureAlg")]
        public int FlowmonTlsSignatureAlg { get; set; }

        [JsonPropertyName("flowmon:tlsPublicKeyAlg")]
        public int FlowmonTlsPublicKeyAlg { get; set; }

        [JsonPropertyName("flowmon:tlsPublicKeyLength")]
        public int FlowmonTlsPublicKeyLength { get; set; }

        [JsonPropertyName("flowmon:tlsJa3Fingerprint")]
        public string FlowmonTlsJa3Fingerprint { get; set; }

        [JsonPropertyName("flowmon:npmJitterDev")]
        public int FlowmonNpmJitterDev { get; set; }

        [JsonPropertyName("flowmon:npmJitterAvg")]
        public int FlowmonNpmJitterAvg { get; set; }

        [JsonPropertyName("flowmon:npmJitterMin")]
        public int FlowmonNpmJitterMin { get; set; }

        [JsonPropertyName("flowmon:npmJitterMax")]
        public int FlowmonNpmJitterMax { get; set; }

        [JsonPropertyName("flowmon:npmDelayDev")]
        public int FlowmonNpmDelayDev { get; set; }

        [JsonPropertyName("flowmon:npmDelayAvg")]
        public int FlowmonNpmDelayAvg { get; set; }

        [JsonPropertyName("flowmon:npmDelayMin")]
        public int FlowmonNpmDelayMin { get; set; }

        [JsonPropertyName("flowmon:npmDelayMax")]
        public int FlowmonNpmDelayMax { get; set; }

        [JsonPropertyName("flowmon:npmTcpRetransmission")]
        public int FlowmonNpmTcpRetransmission { get; set; }

        [JsonPropertyName("flowmon:npmTcpOutOfOrder")]
        public int FlowmonNpmTcpOutOfOrder { get; set; }

        [JsonPropertyName("flowmon:npnRoundTripTime")]
        public int FlowmonNpnRoundTripTime { get; set; }

        [JsonPropertyName("flowmon:npmServerResponseTime")]
        public long FlowmonNpmServerResponseTime { get; set; }
        #endregion
        #region IANA IPFIX Fields
        [JsonPropertyName("@type")]
        public string Type { get; set; }

        [JsonPropertyName("iana:octetDeltaCount")]
        public int IanaOctetDeltaCount { get; set; }

        [JsonPropertyName("iana:packetDeltaCount")]
        public int IanaPacketDeltaCount { get; set; }

        [JsonPropertyName("iana:protocolIdentifier")]
        public string IanaProtocolIdentifier { get; set; }

        [JsonPropertyName("iana:ipClassOfService")]
        public int IanaIpClassOfService { get; set; }

        [JsonPropertyName("iana:tcpControlBits")]
        public string IanaTcpControlBits { get; set; }

        [JsonPropertyName("iana:sourceTransportPort")]
        public int IanaSourceTransportPort { get; set; }

        [JsonPropertyName("iana:sourceIPv4Address")]
        public string IanaSourceIPv4Address { get; set; }

        [JsonPropertyName("iana:ingressInterface")]
        public int IanaIngressInterface { get; set; }

        [JsonPropertyName("iana:destinationTransportPort")]
        public int IanaDestinationTransportPort { get; set; }

        [JsonPropertyName("iana:destinationIPv4Address")]
        public string IanaDestinationIPv4Address { get; set; }

        [JsonPropertyName("iana:egressInterface")]
        public int IanaEgressInterface { get; set; }

        [JsonPropertyName("iana:bgpSourceAsNumber")]
        public int IanaBgpSourceAsNumber { get; set; }

        [JsonPropertyName("iana:bgpDestinationAsNumber")]
        public int IanaBgpDestinationAsNumber { get; set; }

        [JsonPropertyName("iana:samplingInterval")]
        public int IanaSamplingInterval { get; set; }

        [JsonPropertyName("iana:samplingAlgorithm")]
        public int IanaSamplingAlgorithm { get; set; }

        [JsonPropertyName("iana:sourceMacAddress")]
        public string IanaSourceMacAddress { get; set; }

        [JsonPropertyName("iana:postDestinationMacAddress")]
        public string IanaPostDestinationMacAddress { get; set; }

        [JsonPropertyName("iana:ipVersion")]
        public int IanaIpVersion { get; set; }

        [JsonPropertyName("iana:flowStartMilliseconds")]
        public DateTime IanaFlowStartMilliseconds { get; set; }

        [JsonPropertyName("iana:flowEndMilliseconds")]
        public DateTime IanaFlowEndMilliseconds { get; set; }

        [JsonPropertyName("iana:applicationId")]
        public int IanaApplicationId { get; set; }
        #endregion
  
    }

    public static class IpfixcolEntryExtensions
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



