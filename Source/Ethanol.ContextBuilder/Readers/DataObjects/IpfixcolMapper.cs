using System;
using AutoMapper;
using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    public static class IpfixcolMapper
    {
        static IMapper _mapperflow = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<IpfixcolEntry, IpFlow>()
                .ForMember(d => d.FlowType, o => o.MapFrom(s => FlowType.UnidirectionalFlow))
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => 0))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.IanaOctetDeltaCount))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => MapConvert.Flow.Address(s.IanaIpVersion, s.IanaDestinationIPv4Address, s.IanaDestinationIPv6Address)))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.IanaDestinationTransportPort))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => 0))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.IanaPacketDeltaCount))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => MapConvert.Flow.ProtocolType(s.IanaProtocolIdentifier)))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => MapConvert.Flow.ApplicationName(s.IanaApplicationId)))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => MapConvert.Flow.Address(s.IanaIpVersion, s.IanaSourceIPv4Address, s.IanaSourceIPv6Address)))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.IanaSourceTransportPort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.IanaFlowStartMilliseconds))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.IanaFlowEndMilliseconds - s.IanaFlowStartMilliseconds));

            cfg.CreateMap<IpfixcolEntry, DnsFlow>()
                .IncludeBase<IpfixcolEntry, IpFlow>()
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => ApplicationProtocol.DNS))
                .ForMember(d => d.Identifier, o => o.MapFrom(s => s.FlowmonDnsId))
                .ForMember(d => d.QuestionCount, o => o.MapFrom(s => s.FlowmonDnsQuestionCount))
                .ForMember(d => d.AnswerCount, o => o.MapFrom(s => s.FlowmonDnsAnswrecCount))
                .ForMember(d => d.AuthorityCount, o => o.MapFrom(s => s.FlowmonDnsAuthrecCount))
                .ForMember(d => d.AdditionalCount, o => o.MapFrom(s => s.FlowmonDnsAddtrecCount))
                .ForMember(d => d.ResponseType, o => o.MapFrom(s => MapConvert.Dns.RecordType(s.FlowmonDnsCrrType)))
                .ForMember(d => d.ResponseClass, o => o.MapFrom(s => MapConvert.Dns.RecordClass(s.FlowmonDnsCrrClass)))
                .ForMember(d => d.ResponseTTL, o => o.MapFrom(s => s.FlowmonDnsCrrTtl))
                .ForMember(d => d.ResponseName, o => o.MapFrom(s => MapConvert.DecodeString(s.FlowmonDnsCrrName)))
                .ForMember(d => d.ResponseCode, o => o.MapFrom(s => MapConvert.Dns.ResponseCode(s.FlowmonDnsFlagsCodes)))
                .ForMember(d => d.ResponseData, o => o.MapFrom(s => MapConvert.Dns.DecodeDnsRecordData(s.FlowmonDnsCrrRdata, s.FlowmonDnsCrrType)))
                .ForMember(d => d.QuestionType, o => o.MapFrom(s => MapConvert.Dns.RecordType(s.FlowmonDnsQtype)))
                .ForMember(d => d.QuestionClass, o => o.MapFrom(s => MapConvert.Dns.RecordClass(s.FlowmonDnsQclass)))
                .ForMember(d => d.QuestionName, o => o.MapFrom(s => MapConvert.DecodeString(s.FlowmonDnsQname)))
                .ForMember(d => d.Flags, o => o.MapFrom(s => MapConvert.Dns.DnsFlags(s.FlowmonDnsFlagsCodes)))
                .ForMember(d => d.Opcode, o => o.MapFrom(s => MapConvert.Dns.DnsOpcode(s.FlowmonDnsFlagsCodes)))
                .ForMember(d => d.QueryResponseFlag, o => o.MapFrom(s => MapConvert.Dns.QueryResponse(s.FlowmonDnsFlagsCodes)));

            cfg.CreateMap<IpfixcolEntry, HttpFlow>()
                .IncludeBase<IpfixcolEntry, IpFlow>()
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => ApplicationProtocol.HTTP))
                .ForMember(d => d.Hostname, o => o.MapFrom(s => MapConvert.DecodeString(s.FlowmonHttpHost)))
                .ForMember(d => d.Method, o => o.MapFrom(s => MapConvert.Http.MethodMaskString(s.FlowmonHttpMethodMask)))
                .ForMember(d => d.ResultCode, o => o.MapFrom(s => MapConvert.Http.ResultCode(s.FlowmonHttpStatusCode)))
                .ForMember(d => d.Url, o => o.MapFrom(s => MapConvert.Http.UrlString(s.FlowmonHttpUrl)))
                .ForMember(d => d.OperatingSystem, o => o.MapFrom(s => MapConvert.Http.OsString(s.FlowmonHttpUaOs)))
                .ForMember(d => d.ApplicationInformation, o => o.MapFrom(s => MapConvert.Http.UaString(s.FlowmonHttpUaApp)));

            cfg.CreateMap<IpfixcolEntry, TlsFlow>()
                .IncludeBase<IpfixcolEntry, IpFlow>()
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => ApplicationProtocol.SSL))
                .ForMember(h => h.IssuerCommonName, o => o.MapFrom(s => MapConvert.DecodeString(s.FlowmonTlsIssuerCn)))
                .ForMember(h => h.SubjectCommonName, o => o.MapFrom(s => MapConvert.DecodeString(s.FlowmonTlsSubjectCn)))
                .ForMember(h => h.SubjectOrganisationName, o => o.MapFrom(s => MapConvert.DecodeString(s.FlowmonTlsSubjectOn)))
                .ForMember(h => h.CertificateValidityFrom, o => o.MapFrom(s => MapConvert.UnixTimestamp(s.FlowmonTlsValidityNotBefore)))
                 .ForMember(h => h.CertificateValidityTo, o => o.MapFrom(s => MapConvert.UnixTimestamp(s.FlowmonTlsValidityNotAfter)))
                 .ForMember(h => h.SignatureAlgorithm, o => o.MapFrom(s => s.FlowmonTlsSignatureAlg))
                 .ForMember(h => h.PublicKeyAlgorithm, o => o.MapFrom(s => s.FlowmonTlsPublicKeyAlg))
                 .ForMember(h => h.PublicKeyLength, o => o.MapFrom(s => s.FlowmonTlsPublicKeyLength))

                 .ForMember(h => h.ClientVersion, o => o.MapFrom(s => s.FlowmonTlsClientVersion))
                 .ForMember(h => h.CipherSuites, o => o.MapFrom(s => MapConvert.StripPrefix(s.FlowmonTlsCipherSuites)))
                 .ForMember(h => h.ClientRandomID, o => o.MapFrom(s => MapConvert.StripPrefix(s.FlowmonTlsClientRandom)))
                 .ForMember(h => h.ClientSessionID, o => o.MapFrom(s => MapConvert.StripPrefix(s.FlowmonTlsClientSessionId)))
                 .ForMember(h => h.ExtensionTypes, o => o.MapFrom(s => MapConvert.DecodeShortArray(s.FlowmonTlsExtensionTypes)))
                 .ForMember(h => h.ExtensionLengths, o => o.MapFrom(s => MapConvert.DecodeShortArray(s.FlowmonTlsExtensionLengths)))
                 .ForMember(h => h.EllipticCurves, o => o.MapFrom(s => MapConvert.StripPrefix(s.FlowmonTlsEllipticCurves)))
                 .ForMember(h => h.EcPointFormats, o => o.MapFrom(s => MapConvert.StripPrefix(s.FlowmonTlsEcPointFormats)))
                 .ForMember(h => h.ClientKeyLength, o => o.MapFrom(s => s.FlowmonTlsClientKeyLength))
                 .ForMember(h => h.JA3Fingerprint, o => o.MapFrom(s => MapConvert.StripPrefix(s.FlowmonTlsJa3Fingerprint)))

                 .ForMember(h => h.ContentType, o => o.MapFrom(s => s.FlowmonTlsContentType))
                 .ForMember(h => h.HandshakeType, o => o.MapFrom(s => s.FlowmonTlsHandshakeType))
                 .ForMember(h => h.SetupTime, o => o.MapFrom(s => MapConvert.Microseconds(s.FlowmonTlsSetupTime)))
                 .ForMember(h => h.ServerVersion, o => o.MapFrom(s => s.FlowmonTlsServerVersion))
                 .ForMember(h => h.ServerRandomID, o => o.MapFrom(s => MapConvert.StripPrefix(s.FlowmonTlsServerRandom)))
                 .ForMember(h => h.ServerSessionID, o => o.MapFrom(s => MapConvert.StripPrefix(s.FlowmonTlsServerSessionId)))
                 .ForMember(h => h.ServerCipherSuite, o => o.MapFrom(s => MapConvert.HexString(s.FlowmonTlsCipherSuite,4)))
                 .ForMember(h => h.ApplicationLayerProtocolNegotiation, o => o.MapFrom(s => MapConvert.DecodeString(s.FlowmonTlsAlpn)))
                 .ForMember(h => h.ServerNameIndication, o => o.MapFrom(s => MapConvert.DecodeString(s.FlowmonTlsSni)))
                 .ForMember(h => h.ServerNameLength, o => o.MapFrom(s => s.FlowmonTlsSniLength));
        }).CreateMapper();

        public static IpFlow ToFlow(this IpfixcolEntry entry) => MapConvert.Flow.ApplicationName(entry.IanaApplicationId) switch
        {
            ApplicationProtocol.DNS => _mapperflow.Map<IpfixcolEntry, DnsFlow>(entry),
            ApplicationProtocol.HTTP => _mapperflow.Map<IpfixcolEntry, HttpFlow>(entry),
            ApplicationProtocol.SSL => _mapperflow.Map<IpfixcolEntry, TlsFlow>(entry),
            _ => _mapperflow.Map<IpfixcolEntry, IpFlow>(entry)
        };
    }
}



