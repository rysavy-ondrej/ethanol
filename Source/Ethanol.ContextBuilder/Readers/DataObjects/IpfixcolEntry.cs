using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// Representational object for JSON record created by ipfixcol2 tool. 
    /// <para/>
    /// The object is loaded from the output from ipfixcol2 tool. 
    /// The field specification is defined in the following file:
    /// https://github.com/CESNET/libfds/blob/master/config/system/elements/flowmon.xml
    /// </summary>
    public class IpfixcolEntry
    {
        [JsonPropertyName("@type")]
        public string RecordType { get; set; }

        [JsonPropertyName("iana:octetDeltaCount")]
        public long IanaOctetDeltaCount { get; set; }

        [JsonPropertyName("iana:packetDeltaCount")]
        public int IanaPacketDeltaCount { get; set; }

        [JsonPropertyName("iana:protocolIdentifier")]
        public string IanaProtocolIdentifier { get; set; }

        [JsonPropertyName("iana:ipClassOfService")]
        public int IanaIpClassOfService { get; set; }

        [JsonPropertyName("iana:sourceTransportPort")]
        public int IanaSourceTransportPort { get; set; }

        [JsonPropertyName("iana:sourceIPv4Address")]
        public string IanaSourceIPv4Address { get; set; }

        [JsonPropertyName("iana:sourceIPv6Address")]
        public string IanaSourceIPv6Address { get; set; }

        [JsonPropertyName("iana:ingressInterface")]
        public int IanaIngressInterface { get; set; }

        [JsonPropertyName("iana:destinationTransportPort")]
        public int IanaDestinationTransportPort { get; set; }

        [JsonPropertyName("iana:destinationIPv4Address")]
        public string IanaDestinationIPv4Address { get; set; }
                
        [JsonPropertyName("iana:destinationIPv6Address")]
        public string IanaDestinationIPv6Address { get; set; }

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

        [JsonPropertyName("flowmon:npmJitterDev")]
        public long FlowmonNpmJitterDev { get; set; }

        [JsonPropertyName("flowmon:npmJitterAvg")]
        public long FlowmonNpmJitterAvg { get; set; }

        [JsonPropertyName("flowmon:npmJitterMin")]
        public long FlowmonNpmJitterMin { get; set; }

        [JsonPropertyName("flowmon:npmJitterMax")]
        public long FlowmonNpmJitterMax { get; set; }

        [JsonPropertyName("flowmon:npmDelayDev")]
        public long FlowmonNpmDelayDev { get; set; }

        [JsonPropertyName("flowmon:npmDelayAvg")]
        public long FlowmonNpmDelayAvg { get; set; }

        [JsonPropertyName("flowmon:npmDelayMin")]
        public long FlowmonNpmDelayMin { get; set; }

        [JsonPropertyName("flowmon:npmDelayMax")]
        public long FlowmonNpmDelayMax { get; set; }

        [JsonPropertyName("flowmon:npnRoundTripTime")]
        public long FlowmonNpnRoundTripTime { get; set; }

        [JsonPropertyName("flowmon:npmServerResponseTime")]
        public long FlowmonNpmServerResponseTime { get; set; }

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
        public long FlowmonDnsCrrTtl { get; set; }

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

        [JsonPropertyName("iana:tcpControlBits")]
        public string IanaTcpControlBits { get; set; }

        [JsonPropertyName("flowmon:npmTcpRetransmission")]
        public long FlowmonNpmTcpRetransmission { get; set; }

        [JsonPropertyName("flowmon:npmTcpOutOfOrder")]
        public long FlowmonNpmTcpOutOfOrder { get; set; }

        [JsonPropertyName("flowmon:httpHost")]
        public string FlowmonHttpHost { get; set; }

        [JsonPropertyName("flowmon:httpMethodMask")]
        public int FlowmonHttpMethodMask { get; set; }

        [JsonPropertyName("flowmon:tlsContentType")]
        public int FlowmonTlsContentType { get; set; }

        [JsonPropertyName("flowmon:tlsHandshakeType")]
        public int FlowmonTlsHandshakeType { get; set; }

        [JsonPropertyName("flowmon:tlsSetupTime")]
        public long FlowmonTlsSetupTime { get; set; }

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

        /* inconsistent data type: octets vs int -- ignore in input
        [JsonPropertyName("flowmon:tlsEcPointFormats")]
        public long FlowmonTlsEcPointFormats { get; set; }
        */

        [JsonPropertyName("flowmon:tlsClientKeyLength")]
        public int FlowmonTlsClientKeyLength { get; set; }

        [JsonPropertyName("flowmon:tlsIssuerCn")]
        public string FlowmonTlsIssuerCn { get; set; }

        [JsonPropertyName("flowmon:tlsSubjectCn")]
        public string FlowmonTlsSubjectCn { get; set; }

        [JsonPropertyName("flowmon:tlsSubjectOn")]
        public string FlowmonTlsSubjectOn { get; set; }

        [JsonPropertyName("flowmon:tlsValidityNotBefore")]
        public long FlowmonTlsValidityNotBefore { get; set; }

        [JsonPropertyName("flowmon:tlsValidityNotAfter")]
        public long FlowmonTlsValidityNotAfter { get; set; }

        [JsonPropertyName("flowmon:tlsSignatureAlg")]
        public int FlowmonTlsSignatureAlg { get; set; }

        [JsonPropertyName("flowmon:tlsPublicKeyAlg")]
        public int FlowmonTlsPublicKeyAlg { get; set; }

        [JsonPropertyName("flowmon:tlsPublicKeyLength")]
        public int FlowmonTlsPublicKeyLength { get; set; }

        [JsonPropertyName("flowmon:tlsJa3Fingerprint")]
        public string FlowmonTlsJa3Fingerprint { get; set; }

        [JsonPropertyName("flowmon:httpUrl")]
        public string FlowmonHttpUrl { get; set; }

        [JsonPropertyName("flowmon:httpStatusCode")]
        public int FlowmonHttpStatusCode { get; set; }

        [JsonPropertyName("flowmon:httpUaOs")]
        public int FlowmonHttpUaOs { get; set; }

        [JsonPropertyName("flowmon:httpUaOsMaj")]
        public int FlowmonHttpUaOsMaj { get; set; }

        [JsonPropertyName("flowmon:httpUaOsMin")]
        public int FlowmonHttpUaOsMin { get; set; }

        [JsonPropertyName("flowmon:httpUaOsBld")]
        public int FlowmonHttpUaOsBld { get; set; }

        [JsonPropertyName("flowmon:httpUaApp")]
        public int FlowmonHttpUaApp { get; set; }

        [JsonPropertyName("flowmon:httpUaAppMaj")]
        public int FlowmonHttpUaAppMaj { get; set; }

        [JsonPropertyName("flowmon:httpUaAppMin")]
        public int FlowmonHttpUaAppMin { get; set; }

        [JsonPropertyName("flowmon:httpUaAppBld")]
        public int FlowmonHttpUaAppBld { get; set; }

        [JsonPropertyName("flowmon:tcpSynSize")]
        public long FlowmonTcpSynSize { get; set; }

        [JsonPropertyName("flowmon:tcpSynTtl")]
        public long FlowmonTcpSynTtl { get; set; }

        [JsonPropertyName("iana:tcpWindowSize")]
        public long IanaTcpWindowSize { get; set; }
    }
}



