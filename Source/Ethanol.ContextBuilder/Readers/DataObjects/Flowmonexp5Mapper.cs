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
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => s.BytesB))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.BytesA))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => Convert.ToAddress(s.L3Proto, s.L3Ipv4Dst, s.L3Ipv6Dst)))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.L4PortDst))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => s.PacketsB))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.PacketsA))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => Convert.ToProtocolType(s.L4Proto)))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => Convert.ToApplicationProtocol(s.NbarName).ToString()))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => Convert.ToAddress(s.L3Proto, s.L3Ipv4Src, s.L3Ipv6Src)))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.L4PortSrc))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.StartNsec))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.EndNsec - s.StartNsec));

            // Tricky part is the mapping of flags:
            // Flags (2 bytes): A set of flags that control the behavior of the query/response.
            // The flags include:
            //   the QR flag (1 bit),
            //   the Opcode field (4 bits),
            //   the AA flag (1 bit),
            //   the TC flag (1 bit),
            //   the RD flag (1 bit),
            //   the RA flag (1 bit),
            //   the Z field (3 bits), and
            //   the RCODE field (4 bits). 
            cfg.CreateMap<Flowmonexp5Entry, DnsFlow>()
                .IncludeBase<Flowmonexp5Entry, IpFlow>()
                .ForMember(d => d.Identifier, o => o.MapFrom(s => s.InveaDnsId))
                .ForMember(d => d.QuestionCount, o => o.MapFrom(s => Convert.ToInt(s.InveaDnsQuestionCount)))
                .ForMember(d => d.AnswerCount, o => o.MapFrom(s => Convert.ToInt(s.InveaDnsAnswrecCountResponse)))
                .ForMember(d => d.AuthorityCount, o => o.MapFrom(s => Convert.ToInt(s.InveaDnsAuthrecCountResponse)))
                .ForMember(d => d.AdditionalCount, o => o.MapFrom(s => Convert.ToInt(s.InveaDnsAddtrecCountResponse)))
                .ForMember(d => d.ResponseType, o => o.MapFrom(s => Convert.ToInt(s.InveaDnsCrrType)))
                .ForMember(d => d.ResponseClass, o => o.MapFrom(s => Convert.ToInt(s.InveaDnsCrrClass)))
                .ForMember(d => d.ResponseTTL, o => o.MapFrom(s => Convert.ToInt(s.InveaDnsCrrTtl)))
                .ForMember(d => d.ResponseName, o => o.MapFrom(s => Convert.ToString(s.InveaDnsCrrName)))
                .ForMember(d => d.ResponseCode, o => o.MapFrom(s => Convert.ToInt(s.InveaDnsFlagsCodesResponse)))
                .ForMember(d => d.ResponseData, o => o.MapFrom(s => Convert.ToString(s.InveaDnsCrrRdata)))
                .ForMember(d => d.QuestionType, o => o.MapFrom(s => Convert.ToInt(s.InveaDnsQtype)))
                .ForMember(d => d.QuestionClass, o => o.MapFrom(s => Convert.ToInt(s.InveaDnsQclass)))
                .ForMember(d => d.QuestionName, o => o.MapFrom(s => Convert.ToString(s.InveaDnsQname)))
                .ForMember(d => d.Flags, o => o.MapFrom(s => Convert.ToString(s.InveaDnsFlagsCodesRequest)))
                .ForMember(d => d.Opcode, o => o.MapFrom(s => Convert.GetOpcode(s.InveaDnsFlagsCodesRequest)))
                .ForMember(d => d.QueryResponseFlag, o => o.MapFrom(s => DnsQueryResponseFlag.Query));
            
            cfg.CreateMap<Flowmonexp5Entry, HttpFlow>()
                 .IncludeBase<Flowmonexp5Entry, IpFlow>()
                 .ForMember(d => d.Hostname, o => o.MapFrom(s => Convert.ToString(s.HttpRequestHost)))
                 .ForMember(d => d.Method, o => o.MapFrom(s => Convert.ToString(s.HttpMethodMask)))
                 .ForMember(d => d.ResultCode, o => o.MapFrom(s => Convert.ToHttpResultCode(s.HttpResponseStatusCode)))
                 .ForMember(d => d.Url, o => o.MapFrom(s => Convert.ToString(s.HttpRequestUrl)))
                 .ForMember(d => d.OperatingSystem, o => o.MapFrom(s => Convert.ToString(s.HttpUaOs)))
                 .ForMember(d => d.ApplicationInformation, o => o.MapFrom(s => Convert.ToString(s.HttpUaApp)));
            
            cfg.CreateMap<Flowmonexp5Entry, TlsFlow>()
                 .IncludeBase<Flowmonexp5Entry, IpFlow>()
                 .ForMember(h => h.IssuerCommonName, o => o.MapFrom(s => Convert.ToString(s.TlsIssuerCn)))
                 .ForMember(h => h.SubjectCommonName, o => o.MapFrom(s => Convert.ToString(s.TlsSubjectCn)))
                 .ForMember(h => h.SubjectOrganisationName, o => o.MapFrom(s => Convert.ToString(s.TlsSubjectOn)))
                 .ForMember(h => h.CertificateValidityFrom, o => o.MapFrom(s => Convert.FromUnixTimestamp(s.TlsValidityNotBefore)))
                 .ForMember(h => h.CertificateValidityTo, o => o.MapFrom(s => Convert.FromUnixTimestamp(s.TlsValidityNotAfter)))
                 .ForMember(h => h.SignatureAlgorithm, o => o.MapFrom(s => Convert.ToString(s.TlsSignatureAlg)))
                 .ForMember(h => h.PublicKeyAlgorithm, o => o.MapFrom(s => Convert.ToString(s.TlsPublicKeyAlg)))
                 .ForMember(h => h.PublicKeyLength, o => o.MapFrom(s => Convert.ToInt(s.TlsPublicKeyLength)))

                 .ForMember(h => h.ClientVersion, o => o.MapFrom(s => Convert.ToString(s.TlsClientVersion)))
                 .ForMember(h => h.CipherSuites, o => o.MapFrom(s => Convert.ToString(s.TlsCipherSuites)))
                 .ForMember(h => h.ClientRandomID, o => o.MapFrom(s => Convert.ToString(s.TlsClientRandom)))
                 .ForMember(h => h.ClientSessionID, o => o.MapFrom(s => Convert.ToString(s.TlsClientSessionId)))
                 .ForMember(h => h.ExtensionTypes, o => o.MapFrom(s => Convert.ToUShortArray(s.TlsExtensionTypes)))
                 .ForMember(h => h.ExtensionLengths, o => o.MapFrom(s => Convert.ToUShortArray(s.TlsExtensionLengths)))
                 .ForMember(h => h.EllipticCurves, o => o.MapFrom(s => Convert.ToString(s.TlsEllipticCurves)))
                 .ForMember(h => h.EcPointFormats, o => o.MapFrom(s => Convert.ToString(s.TlsEcPointFormats)))
                 .ForMember(h => h.ClientKeyLength, o => o.MapFrom(s => Convert.ToInt(s.TlsClientKeyLength)))
                 .ForMember(h => h.JA3Fingerprint, o => o.MapFrom(s => Convert.ToString(s.TlsJa3Fingerprint)))

                 .ForMember(h => h.ContentType, o => o.MapFrom(s => Convert.ToString(s.TlsContentType)))
                 .ForMember(h => h.HandshakeType, o => o.MapFrom(s => Convert.ToString(s.TlsHandshakeType)))
                 .ForMember(h => h.SetupTime, o => o.MapFrom(s => Convert.FromMicroseconds(s.TlsSetupTime)))
                 .ForMember(h => h.ServerVersion, o => o.MapFrom(s => Convert.ToString(s.TlsServerVersion)))
                 .ForMember(h => h.ServerRandomID, o => o.MapFrom(s => Convert.ToString(s.TlsServerRandom)))
                 .ForMember(h => h.ServerSessionID, o => o.MapFrom(s => Convert.ToString(s.TlsServerSessionId)))
                 .ForMember(h => h.ServerCipherSuite, o => o.MapFrom(s => Convert.ToString(s.TlsCipherSuite)))
                 .ForMember(h => h.ApplicationLayerProtocolNegotiation, o => o.MapFrom(s => String.Empty))
                 .ForMember(h => h.ServerNameIndication, o => o.MapFrom(s => Convert.ToString(s.TlsSni)))
                 .ForMember(h => h.ServerNameLength, o => o.MapFrom(s => Convert.ToInt(s.TlsSniLength)));
        }
        ).CreateMapper();
      
        static class Convert
        {
            public static IPAddress ToAddress(int l3Proto, string value4, string value6) =>
                l3Proto switch
                {
                    4 => IPAddress.TryParse(value4, out var ipv4) ? ipv4 : IPAddress.None,
                    6 => IPAddress.TryParse(value6, out var ipv6) ? ipv6 : IPAddress.IPv6None,
                    _ => IPAddress.None
                };

            /// <summary>
            /// Maps the NBAR text to the applicaiton name.
            /// </summary>
            /// <param name="nbarName"></param>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            public static ApplicationProtocols ToApplicationProtocol(string nbarName) => nbarName switch
            {
                "DNS_TCP" => ApplicationProtocols.DNS,
                "SSL/TLS" => ApplicationProtocols.SSL,
                "HTTPS" => ApplicationProtocols.HTTPS,
                "HTTP" => ApplicationProtocols.HTTP,
                _ => ApplicationProtocols.Other
            };

            /// <summary>
            /// The opcode field is a 4-bit value located in bits 11 to 14 of the flags field.
            /// </summary>
            /// <param name="flags">The flags value is obtained from the DNS message header. </param>
            /// <returns></returns>
            internal static DnsOpCode GetOpcode(long flags)
            {
                ushort opcode = (ushort)((flags >> 11) & 0x0F);
                return (DnsOpCode)opcode;
            }
            /// <summary>
            /// The QR bit is located in bit 15 of the flags field.
            /// </summary>
            /// <param name="flags">The flags value is obtained from the DNS message header. </param>
            /// <returns></returns>
            internal static DnsQueryResponseFlag GetQueryOrresponse(long flags)
            {
                bool qrBit = ((flags >> 15) & 0x01) == 1;
                return qrBit ? DnsQueryResponseFlag.Response : DnsQueryResponseFlag.Query;
            }
            /// <summary>
            /// The RCODE field is a 4-bit value located in the lower 4 bits of the flags field.
            /// </summary>
            /// <param name="flags">The flags value is obtained from the DNS message header. </param>
            /// <returns></returns>
            internal static DnsResponseCode GetRcode(long flags)
            {
                ushort rcode = (ushort)(flags & 0x0F);
                return (DnsResponseCode)rcode;
            }

            internal static DateTime FromUnixTimestamp(long unixTimestamp)
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                return dateTimeOffset.DateTime;
            }

            internal static string ToHttpResultCode(long httpResponseStatusCode)
            {
                return httpResponseStatusCode.ToString();
            }

            internal  static  int ToInt(long value)
            {
                return (int)value;
            }

            internal static ushort[] ToUShortArray(string hexString)
            {
                if (hexString == null) return null;
                ushort[] ushortArray = new ushort[hexString.Length / 4];

                for (int i = 0; i < hexString.Length; i += 4)
                {
                    string hexSubstring = hexString.Substring(i, 4);
                    ushortArray[i / 4] = System.Convert.ToUInt16(hexSubstring, 16);
                }
                return ushortArray;
            }

            internal static ProtocolType ToProtocolType(int l4Proto)
            {
                return (ProtocolType)(l4Proto);
            }

            internal static string ToString(long value)
            {
                return value.ToString();
            }

            internal static string ToString(string value)
            {
                return value;
            }

            internal static TimeSpan FromMicroseconds(long microseconds)
            {
                return TimeSpan.FromMicroseconds(microseconds);
            }
        }

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
        public static IpFlow ToFlow(this Flowmonexp5Entry entry) => Convert.ToApplicationProtocol(entry.NbarName) switch
        {
            ApplicationProtocols.DNS => _mapperflow.Map<Flowmonexp5Entry, DnsFlow>(entry),
            ApplicationProtocols.HTTP => _mapperflow.Map<Flowmonexp5Entry, HttpFlow>(entry),
            ApplicationProtocols.SSL => _mapperflow.Map<Flowmonexp5Entry, TlsFlow>(entry),
            _ => _mapperflow.Map<Flowmonexp5Entry, IpFlow>(entry)
        };
    }
}



