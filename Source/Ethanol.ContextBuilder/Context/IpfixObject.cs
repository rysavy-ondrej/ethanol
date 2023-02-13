using CsvHelper.Configuration.Attributes;
using System;
using System.Net.Sockets;

namespace Ethanol.ContextBuilder.Context
{

    /// <summary>
    /// The IPFIX record is a unified and native type for handling various IPFIX inputs,
    /// this can be produced from different input objects and it is further processes 
    /// for computing the context.
    /// </summary>
    public class IpfixObject : ICloneable
    {
        /// <inheritdoc/>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Gets the key of the flow. 
        /// </summary>
        [Ignore]
        public IpfixKey FlowKey => new IpfixKey { Proto = Protocol.ToString(), SrcIp = SourceIpAddress, SrcPt = SourceTransportPort, DstIp = DestinationIpAddress, DstPt = DestinationPort };

        /// <summary>
        /// Gets or sets the process name if exsited. Otherwise it is null.
        /// </summary>
        [Name("ProcessName")]
        public string ProcessName { get; set; }

        /// <summary>
        /// Represents a recognized application protocol.
        /// </summary>
        public string AppProtoName { get; set; }
        #region Standard flow Fields
        /// <summary>
        /// The L4 protocol name.
        /// </summary>
        public ProtocolType Protocol { get; set; }
        /// <summary>
        /// The source IP address of the flow.
        /// </summary>
        public string SourceIpAddress { get; set; }
        /// <summary>
        /// The source port of the flow.
        /// </summary>
        public int SourceTransportPort { get; set; }
        /// <summary>
        /// The destination address of the flow.
        /// </summary>
        public string DestinationIpAddress { get; set; }
        /// <summary>
        /// The destination port of the flow.
        /// </summary>
        public int DestinationPort { get; set; }
        /// <summary>
        /// The start of the flow.
        /// </summary>
        public DateTime TimeStart { get; set; }
        /// <summary>
        /// The duration of the flow..
        /// </summary>
        public TimeSpan TimeDuration { get; set; }
        /// <summary>
        /// Number of packets of the (bidirectional) flow.
        /// </summary>
        public int Packets { get; set; }
        /// <summary>
        /// Amount of bytes of the (bidirectional) flow.
        /// </summary>
        public int Bytes { get; set; }
        #endregion
        #region DNS Fields
        public string DnsQueryName { get; set; }
        public string DnsResponseData { get; set; }
        #endregion
        #region HTTP Fields
        /// <summary>
        /// The URL of HTTP request.
        /// </summary>
        public string HttpUrl { get; set; }
        /// <summary>
        /// The status code of the HTTP response message. 
        /// </summary>
        public string HttpResponse { get; set; }
        /// <summary>
        /// The request method code.
        /// </summary>
        public string HttpMethod { get; set; }
        /// <summary>
        /// The host name in HTTP request.
        /// </summary>
        public string HttpHost { get; set; }
        #endregion
        #region TLS Fields
        /// <summary>
        /// The TLS version.
        /// </summary>
        public string TlsVersion { get; set; }
        /// <summary>
        /// The client JA3 hash.
        /// </summary>
        public string TlsJa3 { get; set; }
        /// <summary>
        /// The SNI from TLS handshake.
        /// </summary>
        public string TlsServerName { get; set; }
        /// <summary>
        /// The server common name from the TLS/X.509 certificate.
        /// </summary>
        public string TlsServerCommonName { get; set; }
        #endregion
    }
}



