using CsvHelper.Configuration.Attributes;
using System;

namespace Ethanol.Console
{

    /// <summary>
    /// Represents a single record in the collection of TCP connection list.
    /// </summary>
    public partial class SocketRecord
    {
        [Name("FlowKey")]
        public string FlowKeyString { get; set; }

        [Ignore]
        public FlowKey FlowKey => new FlowKey("TCP", LocalAddress, LocalPort, RemoteAddress, RemotePort);

        [Name("LocalAddress")]
        public string LocalAddress {  get; set; }

        [Name("LocalPort")]
        public int LocalPort { get; set; }

        [Name("RemoteAddress")]
        public string RemoteAddress { get; set; }

        [Name("RemotePort")]
        public int RemotePort { get; set; }

        [Name("State")]
        public string State { get; set; }

        [Name("ProcessName")]
        public string ProcessName { get; set; }

        [Name("CreationTime")]
        public DateTime CreationTime { get; set; }

        [Name("CurrentTime")]
        public DateTime CurrentTime { get; set; }
    }

    /// <summary>
    /// The IPFIX record is a unified and native type for handling various IPFIX inputs.
    /// </summary>
    public class IpfixRecord : ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        [Ignore]
        public FlowKey FlowKey => new FlowKey(Protocol, SourceIpAddress, SourceTransportPort, DestinationIpAddress, DestinationPort);

        [Name("ProcessName")]
        public string ProcessName { get; set; }
        public string Protocol { get;  set; }
        public string SourceIpAddress { get;  set; }
        public int SourceTransportPort { get;  set; }
        public string DestinationIpAddress { get;  set; }
        public int DestinationPort { get;  set; }
        public DateTime TimeStart { get; set; }
        public int Packets { get;  set; }
        public int Bytes { get;  set; }
        public TimeSpan TimeDuration { get;  set; }
        public string TlsClientVersion { get;  set; }
        public string HttpUrl { get;  set; }
        public string TlsJa3 { get;  set; }
        public string TlsServerName { get;  set; }
        public string TlsServerCommonName { get;  set; }
        public string DnsQueryName { get;  set; }
        public string DnsResponseData { get;  set; }
        public string HttpMethod { get;  set; }
        public string HttpHost { get;  set; }
        [Ignore]
        public bool IsTlsFlow => TlsClientVersion != "N/A" && TlsClientVersion != "0";
    }
}
    


