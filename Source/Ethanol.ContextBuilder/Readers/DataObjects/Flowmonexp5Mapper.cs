using AutoMapper;
using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Context;
using System;
using System.Net;
using System.Net.Sockets;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// The <see cref="Flowmonexp5Mapper"/> class is responsible for mapping properties of the <see cref="Flowmonexp5Entry"/> 
    /// to different flow types such as <see cref="IpFlow"/>, <see cref="DnsFlow"/>, <see cref="HttpFlow"/>, and <see cref="TlsFlow"/>.
    /// This mapping process is facilitated by AutoMapper.
    /// </summary>
    /// <remarks>
    /// The class includes configurations for AutoMapper to translate fields between <see cref="Flowmonexp5Entry"/> 
    /// and other flow types. It also provides utility conversion methods to help in the transformation process.
    /// </remarks>
    public static class Flowmonexp5Mapper
    {
        static IMapper _mapperflow = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Flowmonexp5Entry, IpFlow>()
                .ForMember(d=>d.FlowType, o=> o.MapFrom(s=>FlowType.BidirectionFlow))
                .ForMember(d => d.Version, o => o.MapFrom(s => s.L3Proto))
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => s.BytesB))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.BytesA))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => MapConvert.Flow.Address(s.L3Proto, s.L3Ipv4Dst, s.L3Ipv6Dst)))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.L4PortDst))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => s.PacketsB))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.PacketsA))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => (ProtocolType)s.L4Proto))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => MapConvert.Flow.ApplicationName(s.NbarAppId)))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => MapConvert.Flow.Address(s.L3Proto, s.L3Ipv4Src, s.L3Ipv6Src)))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.L4PortSrc))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.StartNsec))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.EndNsec - s.StartNsec));

            cfg.CreateMap<Flowmonexp5Entry, DnsFlow>()
                .IncludeBase<Flowmonexp5Entry, IpFlow>()
                .ForMember(d => d.Identifier, o => o.MapFrom(s => s.InveaDnsId))
                .ForMember(d => d.QuestionCount, o => o.MapFrom(s => s.InveaDnsQuestionCount))
                .ForMember(d => d.AnswerCount, o => o.MapFrom(s => s.InveaDnsAnswrecCountResponse))
                .ForMember(d => d.AuthorityCount, o => o.MapFrom(s => s.InveaDnsAuthrecCountResponse))
                .ForMember(d => d.AdditionalCount, o => o.MapFrom(s => s.InveaDnsAddtrecCountResponse))
                .ForMember(d => d.ResponseType, o => o.MapFrom(s => s.InveaDnsCrrType))
                .ForMember(d => d.ResponseClass, o => o.MapFrom(s => s.InveaDnsCrrClass))
                .ForMember(d => d.ResponseTTL, o => o.MapFrom(s => s.InveaDnsCrrTtl))
                .ForMember(d => d.ResponseName, o => o.MapFrom(s => s.InveaDnsCrrName))
                .ForMember(d => d.ResponseCode, o => o.MapFrom(s => MapConvert.Dns.ResponseCode(s.InveaDnsFlagsCodesResponse)))
                .ForMember(d => d.ResponseData, o => o.MapFrom(s => s.InveaDnsCrrRdata))
                .ForMember(d => d.QuestionType, o => o.MapFrom(s => s.InveaDnsQtype))
                .ForMember(d => d.QuestionClass, o => o.MapFrom(s => s.InveaDnsQclass))
                .ForMember(d => d.QuestionName, o => o.MapFrom(s => s.InveaDnsQname))
                .ForMember(d => d.Flags, o => o.MapFrom(s => s.InveaDnsFlagsCodesRequest))
                .ForMember(d => d.Opcode, o => o.MapFrom(s => MapConvert.Dns.DnsOpcode(s.InveaDnsFlagsCodesRequest)))
                .ForMember(d => d.QueryResponseFlag, o => o.MapFrom(s => MapConvert.Dns.QueryResponse(s.InveaDnsFlagsCodesResponse)));
            
            cfg.CreateMap<Flowmonexp5Entry, HttpFlow>()
                 .IncludeBase<Flowmonexp5Entry, IpFlow>()
                 .ForMember(d => d.Hostname, o => o.MapFrom(s => s.HttpRequestHost))
                 .ForMember(d => d.Method, o => o.MapFrom(s => MapConvert.Http.MethodMaskString(s.HttpMethodMask)))
                 .ForMember(d => d.ResultCode, o => o.MapFrom(s => MapConvert.Http.ResultCode(s.HttpResponseStatusCode)))
                 .ForMember(d => d.Url, o => o.MapFrom(s => s.HttpRequestUrl))
                 .ForMember(d => d.OperatingSystem, o => o.MapFrom(s => s.HttpUaOs))
                 .ForMember(d => d.ApplicationInformation, o => o.MapFrom(s => s.HttpUaApp));
            
            cfg.CreateMap<Flowmonexp5Entry, TlsFlow>()
                 .IncludeBase<Flowmonexp5Entry, IpFlow>()
                 .ForMember(h => h.IssuerCommonName, o => o.MapFrom(s => s.TlsIssuerCn))
                 .ForMember(h => h.SubjectCommonName, o => o.MapFrom(s => s.TlsSubjectCn))
                 .ForMember(h => h.SubjectOrganisationName, o => o.MapFrom(s => s.TlsSubjectOn))
                 .ForMember(h => h.CertificateValidityFrom, o => o.MapFrom(s => MapConvert.UnixTimestamp(s.TlsValidityNotBefore)))
                 .ForMember(h => h.CertificateValidityTo, o => o.MapFrom(s => MapConvert.UnixTimestamp(s.TlsValidityNotAfter)))
                 .ForMember(h => h.SignatureAlgorithm, o => o.MapFrom(s => s.TlsSignatureAlg))
                 .ForMember(h => h.PublicKeyAlgorithm, o => o.MapFrom(s => s.TlsPublicKeyAlg))
                 .ForMember(h => h.PublicKeyLength, o => o.MapFrom(s => s.TlsPublicKeyLength))

                 .ForMember(h => h.ClientVersion, o => o.MapFrom(s => s.TlsClientVersion))
                 .ForMember(h => h.CipherSuites, o => o.MapFrom(s => s.TlsCipherSuites))
                 .ForMember(h => h.ClientRandomID, o => o.MapFrom(s => s.TlsClientRandom))
                 .ForMember(h => h.ClientSessionID, o => o.MapFrom(s => s.TlsClientSessionId))
                 .ForMember(h => h.ExtensionTypes, o => o.MapFrom(s => MapConvert.DecodeShortArray(s.TlsExtensionTypes)))
                 .ForMember(h => h.ExtensionLengths, o => o.MapFrom(s => MapConvert.DecodeShortArray(s.TlsExtensionLengths)))
                 .ForMember(h => h.EllipticCurves, o => o.MapFrom(s => s.TlsEllipticCurves))
                 .ForMember(h => h.EcPointFormats, o => o.MapFrom(s => s.TlsEcPointFormats))
                 .ForMember(h => h.ClientKeyLength, o => o.MapFrom(s => s.TlsClientKeyLength))
                 .ForMember(h => h.JA3Fingerprint, o => o.MapFrom(s => s.TlsJa3Fingerprint))
                 .ForMember(h => h.ContentType, o => o.MapFrom(s => s.TlsContentType))
                 .ForMember(h => h.HandshakeType, o => o.MapFrom(s => (s.TlsHandshakeType)))
                 .ForMember(h => h.SetupTime, o => o.MapFrom(s => MapConvert.Microseconds(s.TlsSetupTime)))
                 .ForMember(h => h.ServerVersion, o => o.MapFrom(s => MapConvert.HexString(s.TlsServerVersion,4)))
                 .ForMember(h => h.ServerRandomID, o => o.MapFrom(s => (s.TlsServerRandom)))
                 .ForMember(h => h.ServerSessionID, o => o.MapFrom(s => (s.TlsServerSessionId)))
                 .ForMember(h => h.ServerCipherSuite, o => o.MapFrom(s => MapConvert.HexString(s.TlsCipherSuite,4)))
                 .ForMember(h => h.ApplicationLayerProtocolNegotiation, o => o.MapFrom(s => s.TlsAlpn))
                 .ForMember(h => h.ServerNameIndication, o => o.MapFrom(s => (s.TlsSni)))
                 .ForMember(h => h.ServerNameLength, o => o.MapFrom(s => s.TlsSniLength));
        }
        ).CreateMapper();

        /// <summary>
        /// Maps the NBAR text to the applicaiton name.
        /// </summary>
        /// <param name="nbarName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static ApplicationProtocol GetNbarProtocol(string nbarName) => nbarName switch
        {
            "DNS_TCP" => ApplicationProtocol.DNS,
            "SSL/TLS" => ApplicationProtocol.SSL,
            "HTTPS" => ApplicationProtocol.HTTPS,
            "HTTP" => ApplicationProtocol.HTTP,
            _ => ApplicationProtocol.Other
        };

        /// <summary>
        /// Transforms a <see cref="Flowmonexp5Entry"/> instance into its corresponding <see cref="IpFlow"/> representation. 
        /// This method aims to make the conversion from raw flow data to specific application protocols seamless, 
        /// providing a standardized format for data handling across the application.
        /// </summary>
        /// <remarks>
        /// The transformation is based on the 'NbarName' property of the <see cref="Flowmonexp5Entry"/>, 
        /// which denotes the application protocol used in the flow. Depending on this protocol, the method 
        /// uses the appropriate mapper to create an instance of a specialized flow type (e.g., DnsFlow, HttpFlow, TlsFlow). 
        /// If the protocol doesn't match any predefined types, it defaults to a general <see cref="IpFlow"/>.
        /// 
        /// </remarks>
        /// <param name="entry">The nfdump flow, representing raw flow data to be converted.</param>
        /// <returns>A specialized or general <see cref="IpFlow"/> representation of the provided <see cref="Flowmonexp5Entry"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided <see cref="Flowmonexp5Entry"/> is null.</exception>
        /// <exception cref="NotSupportedException">Thrown when the 'NbarName' does not correspond to a known application protocol and cannot be mapped.</exception>
        public static IpFlow ToFlow(this Flowmonexp5Entry entry) => MapConvert.Flow.ApplicationName(entry.NbarAppId) switch
        {
            ApplicationProtocol.DNS => _mapperflow.Map<Flowmonexp5Entry, DnsFlow>(entry),
            ApplicationProtocol.HTTP => _mapperflow.Map<Flowmonexp5Entry, HttpFlow>(entry),
            ApplicationProtocol.SSL => _mapperflow.Map<Flowmonexp5Entry, TlsFlow>(entry),
            _ => _mapperflow.Map<Flowmonexp5Entry, IpFlow>(entry)
        };
    }
}



