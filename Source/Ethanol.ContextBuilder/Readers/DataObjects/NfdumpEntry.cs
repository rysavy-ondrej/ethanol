using CsvHelper.Configuration.Attributes;
using System.Net.Sockets;
using System;
using AutoMapper;
using Ethanol.ContextBuilder.Context;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        public string Protocol { get; set; }

        [Name("sa")]
        public string SourceIpAddress { get; set; }

        [Name("sp")]
        public int SourceTransportPort { get; set; }

        [Name("da")]
        public string DestinationIpAddress { get; set; }

        [Name("dp")]
        public int DestinationPort { get; set; }

        [Name("ts")]
        public DateTime TimeStart { get; set; }

        [Name("td")]
        public double TimeDuration { get; set; }

        [Name("ipkt")]
        public int InPackets { get; set; }

        [Name("ibyt")]
        public int InBytes { get; set; }

        [Name("opkt")]
        public int OutPackets { get; set; }

        [Name("obyt")]
        public int OutBytes { get; set; }

        public int Packets => InPackets + OutPackets;
        public int Bytes => InBytes + OutBytes;

        [Name("apptag")]
        public string Nbar { get; set; }

        [Name("tlssver")]
        public string TlsVersion { get; set; }

        [Name("hurl")]
        public string HttpUrl { get; set; }

        [Name("hrcode")]
        public int HttpResponse { get; set; }

        [Name("tlsja3")]
        public string TlsJa3 { get; set; }

        [Name("tlssni")]
        public string TlsServerName { get; set; }

        [Name("tlsscn")]
        public string TlsServerCommonName { get; set; }

        [Name("dnsqname")]
        public string DnsQueryName { get; set; }

        [Name("dnsrdata")]
        public string DnsResponseData { get; set; }

        [Name("hmethod")]
        public string HttpMethod { get; set; }

        [Name("hhost")]
        public string HttpHost { get; set; }
    }
    static class NfdumpToIpfixMapper
    {
        static IMapper _mapper = new MapperConfiguration(cfg =>
            cfg.CreateMap<NfdumpEntry, IpfixObject>()
            .ForMember(d => d.Bytes, o => o.MapFrom(s => s.Bytes))
            .ForMember(d => d.DestinationIpAddress, o => o.MapFrom(s => s.DestinationIpAddress))
            .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.DestinationPort))
            .ForMember(d => d.DnsQueryName, o => o.MapFrom(s => s.DnsQueryName))
            .ForMember(d => d.DnsResponseData, o => o.MapFrom(s => s.DnsResponseData))
            .ForMember(d => d.HttpHost, o => o.MapFrom(s => s.HttpHost))
            .ForMember(d => d.HttpMethod, o => o.MapFrom(s => NormalizeString(s.HttpMethod)))
            .ForMember(d => d.HttpResponse, o => o.MapFrom(s => GetResponseCodeString(s.HttpResponse)))
            .ForMember(d => d.HttpUrl, o => o.MapFrom(s => s.HttpUrl))
            .ForMember(d => d.Packets, o => o.MapFrom(s => s.Packets))
            .ForMember(d => d.Protocol, o => o.MapFrom(s => GetProtocol(s.Protocol)))
            .ForMember(d => d.Nbar, o => o.MapFrom(s => s.Nbar))
            .ForMember(d => d.SourceIpAddress, o => o.MapFrom(s => s.SourceIpAddress))
            .ForMember(d => d.SourceTransportPort, o => o.MapFrom(s => s.SourceTransportPort))
            .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.TimeStart))
            .ForMember(d => d.TimeDuration, o => o.MapFrom(s => TimeSpan.FromSeconds(s.TimeDuration)))
            .ForMember(d => d.TlsVersion, o => o.MapFrom(s => NormalizeString(s.TlsVersion)))
            .ForMember(d => d.TlsJa3, o => o.MapFrom(s => NormalizeString(s.TlsJa3)))
            .ForMember(d => d.TlsServerCommonName, o => o.MapFrom(s => NormalizeString(s.TlsServerCommonName)))
            .ForMember(d => d.TlsServerName, o => o.MapFrom(s => NormalizeString(s.TlsServerName)))
            ).CreateMapper();

        private static string NormalizeString(string value)
        {
            return (value == "N/A" || value == "NONE") ? String.Empty : value;
        }

        private static ProtocolType GetProtocol(string protocol)
        {
            return Enum.TryParse<ProtocolType>(protocol, true, out var protocolType) ? protocolType : ProtocolType.Unknown;
        }

        private static string GetResponseCodeString(int httpResponse)
        {
            return (httpResponse > 0) ? httpResponse.ToString() : String.Empty;
        }

        /// <summary>
        /// Get <see cref="IpfixObject"/> from the current <see cref="NfdumpEntry"/>.
        /// </summary>
        /// <param name="entry">The nfdump flow.</param>
        /// <returns><see cref="IpfixObject"/> created from the current <see cref="NfdumpEntry"/>.</returns>
        public static IpfixObject ToIpfix(this NfdumpEntry entry) => _mapper.Map<NfdumpEntry, IpfixObject>(entry);
    }
}



