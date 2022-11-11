using CsvHelper.Configuration.Attributes;
using System;
using System.Net.Sockets;

namespace Ethanol.Console
{

    /// <summary>
    /// The IPFIX record is a unified and native type for handling various IPFIX inputs,
    /// this can be produced from different input obejcts and it is further processes 
    /// for computing the context.
    /// </summary>
    public class IpfixRecord : ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        [Ignore]
        public FlowKey FlowKey => new FlowKey { Proto = Protocol.ToString(), SrcIp = SourceIpAddress, SrcPt = SourceTransportPort, DstIp = DestinationIpAddress, DstPt = DestinationPort };

        [Name("ProcessName")]
        public string ProcessName { get; set; }

        /// <summary>
        /// Represents a string of recognized protocol using NBAR technique.
        /// </summary>
        public string Nbar { get; set; }
        // we need to be sure that the name of the protocol is correct...
        public System.Net.Sockets.ProtocolType Protocol { get;  set; }
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

        public string HttpResponse { get; set; }
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
    


