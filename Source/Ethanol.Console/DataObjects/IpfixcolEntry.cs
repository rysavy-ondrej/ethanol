using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ethanol.ContextBuilder.DataObjects
{
    /// <summary>
    /// POCO for JSON record created by ipfixcol2 tool. 
    /// </summary>
    public partial class IpfixcolEntry
    {
        public static bool TryDeserialize(string input, out IpfixcolEntry entry)
        {
            try
            {
                entry = JsonSerializer.Deserialize<IpfixcolEntry>(input);
                return true;
            }
            catch(Exception)
            {
                entry = default;
                return false;
            }
        }
    }

    public partial class IpfixcolEntry
    {
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
    }
    public partial class IpfixcolEntry
    {

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
    }

    public partial class IpfixcolEntry
    {

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
    }

    /// <summary>
    /// Represents Ipfix.entry record as provided by the JSON output from ipfixcol2 tool.
    /// </summary>
    public partial class IpfixcolEntry
    {
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
    }
}



