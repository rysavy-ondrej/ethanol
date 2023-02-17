using AutoMapper;
using CsvHelper.Configuration.Attributes;
using Ethanol.ContextBuilder.Context;
using System;
using System.Net;
using System.Net.Sockets;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// Represents a single flow record as exported from nfdump with "-o csv" option.
    /// <para/>
    /// The raw CSV ouput from nfdump contains many columns most of them are not used in this tool. 
    /// However, this is the easies and possibly the best option to get the output from nfdump version as 
    /// used in Flowmon. 
    /// <para/>
    /// This class is supposed to be used with CsvHelper library. 
    /// </summary>
    internal class NfdumpEntry
    {
        [Name("pr")]
        public string pr { get; set; }

        [Name("sa")]
        public string sa { get; set; }

        [Name("sp")]
        public int sp { get; set; }

        [Name("da")]
        public string da { get; set; }

        [Name("dp")]
        public int dp { get; set; }

        [Name("ts")]
        public string ts { get; set; }

        [Name("td")]
        public double td { get; set; }

        [Name("ipkt")]
        public int ipkt { get; set; }

        [Name("ibyt")]
        public int ibyt { get; set; }

        [Name("opkt")]
        public int opkt { get; set; }

        [Name("obyt")]
        public int obyt { get; set; }

        [Name("apptag")]
        public string apptag { get; set; }

        #region TLS Fields

        [Name("tlscont")]
        public string tlscont { get; set; }
        [Name("tlshshk")]
        public string tlshshk { get; set; }
        [Name("tlssetup")]
        public string tlssetup { get; set; }
        [Name("tlssver")]
        public string tlssver { get; set; }
        [Name("tlsciph")]
        public string tlsciph { get; set; }
        [Name("tlssrnd")]
        public string tlssrnd { get; set; }
        [Name("tlsssid")]
        public string tlsssid { get; set; }
        [Name("tlsalpn")]
        public string tlsalpn { get; set; }
        [Name("tlssni")]
        public string tlssni { get; set; }

        [Name("tlssnlen")]
        public string tlssnlen { get; set; }
        [Name("tlscver")]
        public string tlscver { get; set; }
        [Name("tlsciphs")]
        public string tlsciphs { get; set; }
        [Name("tlscrnd")]
        public string tlscrnd { get; set; }
        [Name("tlscsid")]
        public string tlscsid { get; set; }
        [Name("tlsext")]
        public string tlsext { get; set; }
        [Name("tlsexl")]
        public string tlsexl { get; set; }
        [Name("tlsec")]
        public string tlsec { get; set; }
        [Name("tlsecpf")]
        public string tlsecpf { get; set; }

        [Name("tlscklen")]
        public string tlscklen { get; set; }
        [Name("tlsicn")]
        public string tlsicn { get; set; }
        [Name("tlsscn")]
        public string tlsscn { get; set; }
        [Name("tlsson")]
        public string tlsson { get; set; }
        [Name("tlsvfrom")]
        public string tlsvfrom { get; set; }
        [Name("tlsvto")]
        public string tlsvto { get; set; }
        [Name("tlssalg")]
        public string tlssalg { get; set; }
        [Name("tlspkalg")]
        public string tlspkalg { get; set; }
        [Name("tlspklen")]
        public string tlspklen { get; set; }

        [Name("tlsja3")]
        public string tlsja3 { get; set; }
        [Name("tlssnum")]
        public string tlssnum { get; set; }
        [Name("tlssan")]
        public string tlssan { get; set; }
        [Name("tlsscm")]
        public string tlsscm { get; set; }
        #endregion


        #region DNS Fields
        [Name("dnsqname")]
        public string dnsqname { get; set; }
        [Name("dnsrdata")]
        public string dnsrdata { get; set; }
        [Name("dnsid")]
        public string dnsid { get; set; }
        [Name("dnsrcode")]
        public string dnsrcode { get; set; }
        [Name("dnsopcode")]
        public string dnsopcode { get; set; }
        [Name("dnsqrflag")]
        public string dnsqrflag { get; set; }
        [Name("dnsflags")]
        public string dnsflags { get; set; }
        [Name("dnsqcnt")]
        public string dnsqcnt { get; set; }
        [Name("dnsancnt")]
        public string dnsancnt { get; set; }
        [Name("dnsaucnt")]
        public string dnsaucnt { get; set; }
        [Name("dnsadcnt")]
        public string dnsadcnt { get; set; }
        [Name("dnsrname")]
        public string dnsrname { get; set; }
        [Name("dnsrclass")]
        public string dnsrclass { get; set; }
        [Name("dnsrtype")]
        public string dnsrtype { get; set; }
        [Name("dnsrttl")]
        public string dnsrttl { get; set; }
        [Name("dnsqtype")]
        public string dnsqtype { get; set; }
        [Name("dnsqclass")]
        public string dnsqclass { get; set; }
        #endregion

        #region HTTP Fields

        [Name("hmethod")]
        public string hmethod { get; set; }
        [Name("hhost")]
        public string hhost { get; set; }
        [Name("hurl")]
        public string hurl { get; set; }
        [Name("hrcode")]
        public int hrcode { get; set; }
        [Name("hos")]
        public string hos { get; set; }
        [Name("happ")]
        public string happ { get; set; }
        #endregion
    }
    static class NfdumpToIpfixMapper
    {

        static IMapper _mapper = new MapperConfiguration(cfg =>
        { 
            cfg.CreateMap<NfdumpEntry, IpFlow>()
                .ForMember(d => d.OctetDeltaCount,      o => o.MapFrom(s => s.ibyt))
                .ForMember(d => d.DestinationAddress,   o => o.MapFrom(s => Convert.ToAddress(s.da)))
                .ForMember(d => d.DestinationPort,      o => o.MapFrom(s => s.dp))
                .ForMember(d => d.PacketDeltaCount,     o => o.MapFrom(s => s.ipkt))
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
                    .ForMember(h => h.ExtensionTypes, o => o.MapFrom(s => Convert.ToString(s.tlsext)))
                    .ForMember(h => h.ExtensionLengths, o => o.MapFrom(s => Convert.ToInt(s.tlsexl)))
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
        }

        /// <summary>
        /// Get <see cref="IpFlow"/> from the current <see cref="NfdumpEntry"/>.
        /// </summary>
        /// <param name="entry">The nfdump flow.</param>
        /// <returns><see cref="IpFlow"/> created from the current <see cref="NfdumpEntry"/>.</returns>
        public static IpFlow ConvertToFlow(this NfdumpEntry entry) => Convert.ToApplicationProtocol(entry.apptag) switch
        {
            ApplicationProtocols.DNS => _mapper.Map<NfdumpEntry, DnsFlow>(entry),
            ApplicationProtocols.HTTP => _mapper.Map<NfdumpEntry, HttpFlow>(entry),
            ApplicationProtocols.SSL => _mapper.Map<NfdumpEntry, TlsFlow>(entry),
            _ => _mapper.Map<NfdumpEntry, IpFlow>(entry)
        };
    }
}



