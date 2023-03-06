using AutoMapper;
using Ethanol.ContextBuilder.Context;
using System;
using System.Net;
using System.Net.Sockets;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// Maps the <see cref="NfdumpEntry"/> to <see cref="IpFlow"/>
    /// <para/>
    /// Nfdump entries are uniflow, but for some extensions they contain information from bout directions. In particular TLS and HTTP representes this case.
    /// </summary>
    static class NfdumpEntryMapper
    {

        static IMapper _mapper = new MapperConfiguration(cfg =>
        { 
            cfg.CreateMap<NfdumpEntry, IpFlow>()
                .ForMember(d => d.FlowType,             o => o.MapFrom(s => Convert.GetFlowType(s)))
                .ForMember(d => d.SentOctets,           o => o.MapFrom(s => s.ibyt))
                .ForMember(d => d.DestinationAddress,   o => o.MapFrom(s => Convert.ToAddress(s.da)))
                .ForMember(d => d.DestinationPort,      o => o.MapFrom(s => s.dp))
                .ForMember(d => d.SentPackets,          o => o.MapFrom(s => s.ipkt))
                .ForMember(d => d.Protocol,             o => o.MapFrom(s => Convert.ToProtocolType(s.pr)))
                .ForMember(d => d.ApplicationTag,       o => o.MapFrom(s => Convert.ToApplicationProtocol(s.apptag).ToString()))
                .ForMember(d => d.SourceAddress,        o => o.MapFrom(s => Convert.ToAddress(s.sa)))
                .ForMember(d => d.SourcePort,           o => o.MapFrom(s => s.sp))
                .ForMember(d => d.TimeStart,            o => o.MapFrom(s => Convert.ToDateTime(s.ts)))
                .ForMember(d => d.TimeDuration,         o => o.MapFrom(s => Convert.ToTimeSpan(s.td)));
            cfg.CreateMap<NfdumpEntry, DnsFlow>()
                .IncludeBase<NfdumpEntry, IpFlow>()
                .ForMember(d => d.Identifier,       o => o.MapFrom(s => s.dnsid))
                .ForMember(d => d.QuestionCount,    o => o.MapFrom(s => Convert.ToInt(s.dnsqcnt)))
                .ForMember(d => d.AnswerCount,      o => o.MapFrom(s => Convert.ToInt(s.dnsancnt)))
                .ForMember(d => d.AuthorityCount,   o => o.MapFrom(s => Convert.ToInt(s.dnsaucnt)))
                .ForMember(d => d.AdditionalCount,  o => o.MapFrom(s => Convert.ToInt(s.dnsadcnt)))
                .ForMember(d => d.ResponseType,     o => o.MapFrom(s => Convert.ToInt(s.dnsrtype)))
                .ForMember(d => d.ResponseClass,    o => o.MapFrom(s => Convert.ToInt(s.dnsrclass)))
                .ForMember(d => d.ResponseTTL,      o => o.MapFrom(s => Convert.ToInt(s.dnsrttl)))
                .ForMember(d => d.ResponseName,     o => o.MapFrom(s => Convert.ToString(s.dnsrname)))
                .ForMember(d => d.ResponseCode,     o => o.MapFrom(s => Convert.ToInt(s.dnsrcode)))
                .ForMember(d => d.ResponseData,     o => o.MapFrom(s => Convert.ToString(s.dnsrdata)))
                .ForMember(d => d.QuestionType,     o => o.MapFrom(s => Convert.ToInt(s.dnsqtype)))
                .ForMember(d => d.QuestionClass,    o => o.MapFrom(s => Convert.ToInt(s.dnsqclass)))
                .ForMember(d => d.QuestionName,     o => o.MapFrom(s => Convert.ToString(s.dnsqname)))
                .ForMember(d => d.Flags,            o => o.MapFrom(s => Convert.ToString(s.dnsflags)))
                .ForMember(d => d.Opcode,           o => o.MapFrom(s => Convert.ToInt(s.dnsopcode)))
                .ForMember(d => d.QueryResponseFlag,o => o.MapFrom(s => Convert.ToString(s.dnsqrflag)));

            cfg.CreateMap<NfdumpEntry, HttpFlow>()
                .IncludeBase<NfdumpEntry, IpFlow>()
                .ForMember(d => d.Hostname,                 o => o.MapFrom(s => Convert.ToString(s.hhost)))
                .ForMember(d => d.Method,                   o => o.MapFrom(s => Convert.ToString(s.hmethod)))
                .ForMember(d => d.ResultCode,               o => o.MapFrom(s => Convert.ToHttpResultCode(s.hrcode)))
                .ForMember(d => d.Url,                      o => o.MapFrom(s => Convert.ToString(s.hurl)))
                .ForMember(d => d.OperatingSystem,          o => o.MapFrom(s => Convert.ToString(s.hos)))
                .ForMember(d => d.ApplicationInformation,   o => o.MapFrom(s => Convert.ToString(s.happ)));

            cfg.CreateMap<NfdumpEntry, TlsFlow>()
                    .IncludeBase<NfdumpEntry, IpFlow>()
                    .ForMember(h => h.IssuerCommonName, o => o.MapFrom(s => Convert.ToString(s.tlsicn)))
                    .ForMember(h => h.SubjectCommonName, o => o.MapFrom(s => Convert.ToString(s.tlsscn)))
                    .ForMember(h => h.SubjectOrganisationName, o => o.MapFrom(s => Convert.ToString(s.tlsson)))
                    .ForMember(h => h.CertificateValidityFrom, o => o.MapFrom(s => Convert.ToDateTime(s.tlsvfrom)))
                    .ForMember(h => h.CertificateValidityTo, o => o.MapFrom(s => Convert.ToDateTime(s.tlsvto)))
                    .ForMember(h => h.SignatureAlgorithm, o => o.MapFrom(s => Convert.ToString(s.tlssalg)))
                    .ForMember(h => h.PublicKeyAlgorithm, o => o.MapFrom(s => Convert.ToString(s.tlspkalg)))
                    .ForMember(h => h.PublicKeyLength, o => o.MapFrom(s => Convert.ToInt(s.tlspklen)))

                    .ForMember(h => h.ClientVersion, o => o.MapFrom(s => Convert.ToString(s.tlscver)))
                    .ForMember(h => h.CipherSuites, o => o.MapFrom(s => Convert.ToString(s.tlsciphs)))
                    .ForMember(h => h.ClientRandomID, o => o.MapFrom(s => Convert.ToString(s.tlscrnd)))
                    .ForMember(h => h.ClientSessionID, o => o.MapFrom(s => Convert.ToString(s.tlscsid)))
                    .ForMember(h => h.ExtensionTypes, o => o.MapFrom(s => Convert.ToUShortArray(s.tlsext)))
                    .ForMember(h => h.ExtensionLengths, o => o.MapFrom(s => Convert.ToUShortArray(s.tlsexl)))
                    .ForMember(h => h.EllipticCurves, o => o.MapFrom(s => Convert.ToString(s.tlsec)))
                    .ForMember(h => h.EcPointFormats, o => o.MapFrom(s => Convert.ToString(s.tlsecpf)))
                    .ForMember(h => h.ClientKeyLength, o => o.MapFrom(s => Convert.ToInt(s.tlscklen)))
                    .ForMember(h => h.JA3Fingerprint, o => o.MapFrom(s => Convert.ToString(s.tlsja3)))

                    .ForMember(h => h.ContentType, o => o.MapFrom(s => Convert.ToString(s.tlscont)))
                    .ForMember(h => h.HandshakeType, o => o.MapFrom(s => Convert.ToString(s.tlshshk)))
                    .ForMember(h => h.SetupTime, o => o.MapFrom(s => Convert.ToTimeSpan(s.tlssetup)))
                    .ForMember(h => h.ServerVersion, o => o.MapFrom(s => Convert.ToString(s.tlssver)))
                    .ForMember(h => h.ServerRandomID, o => o.MapFrom(s => Convert.ToString(s.tlssrnd)))
                    .ForMember(h => h.ServerSessionID, o => o.MapFrom(s => Convert.ToString(s.tlsssid)))
                    .ForMember(h => h.ServerCipherSuite, o => o.MapFrom(s => Convert.ToString(s.tlsciph)))
                    .ForMember(h => h.ApplicationLayerProtocolNegotiation, o => o.MapFrom(s => Convert.ToString(s.tlsalpn)))
                    .ForMember(h => h.ServerNameIndication, o => o.MapFrom(s => Convert.ToString(s.tlssni)))
                    .ForMember(h => h.ServerNameLength, o => o.MapFrom(s => Convert.ToInt(s.tlssnlen)));

        }).CreateMapper();

        static class Convert
        {
            public static ApplicationProtocols ToApplicationProtocol(string nbar) => nbar switch
            {
                "3:443" => ApplicationProtocols.HTTPS,
                "13:453" => ApplicationProtocols.SSL,
                "3:53" => ApplicationProtocols.DNS,
                "3:80" => ApplicationProtocols.HTTP,
                _ => ApplicationProtocols.Other
            };

            public static string ToString(string value)
            {
                return (value == "N/A" || value == "NONE") ? String.Empty : value;
            }
            public static IPAddress ToAddress(string value)
            {
                return IPAddress.TryParse(value, out var result) ? result : IPAddress.None;
            }
            public static int ToInt(string value)
            {
                return Int32.TryParse(value, out var result) ? result : 0;
            }
            public static DateTime ToDateTime(string value)
            {
                return DateTime.TryParse(value, out var result) ? result : DateTime.MinValue;
            }

            public static TimeSpan ToTimeSpan(double value)
            {
                return TimeSpan.FromSeconds(value);
            }
            public static TimeSpan ToTimeSpan(string value)
            {
                var d = double.TryParse(value, out var x)? x : 0;
                return TimeSpan.FromSeconds(d);
            }

            public static ProtocolType ToProtocolType(string protocol)
            {
                return Enum.TryParse<ProtocolType>(protocol, true, out var protocolType) ? protocolType : ProtocolType.Unknown;
            }

            public static string ToHttpResultCode(int httpResponse)
            {
                return (httpResponse > 0) ? httpResponse.ToString() : String.Empty;
            }

            internal static FlowType GetFlowType(NfdumpEntry arg)
            {
                return arg.sp > arg.dp ? FlowType.RequestFlow : FlowType.ResponseFlow;
            }

            internal static ushort[] ToUShortArray(string hexString)
            {
                return Array.Empty<ushort>();
            }
        }

        /// <summary>
        /// Get <see cref="IpFlow"/> from the current <see cref="NfdumpEntry"/>.
        /// </summary>
        /// <param name="entry">The nfdump flow.</param>
        /// <returns><see cref="IpFlow"/> created from the current <see cref="NfdumpEntry"/>.</returns>
        public static IpFlow ToFlow(this NfdumpEntry entry) => Convert.ToApplicationProtocol(entry.apptag) switch
        {
            ApplicationProtocols.DNS => _mapper.Map<NfdumpEntry, DnsFlow>(entry),
            ApplicationProtocols.HTTP => _mapper.Map<NfdumpEntry, HttpFlow>(entry),
            ApplicationProtocols.SSL => _mapper.Map<NfdumpEntry, TlsFlow>(entry),
            _ => _mapper.Map<NfdumpEntry, IpFlow>(entry)
        };
    }
}



