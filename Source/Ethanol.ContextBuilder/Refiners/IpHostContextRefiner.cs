using Ethanol.DataObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ethanol.ContextBuilder.Refiners
{
    /// <summary>
    /// Represents a refiner that transform a time range of <see cref="IpHostContextWithTags"/> into a <see cref="HostContext"/>.
    /// </summary>
    public class IpHostContextRefiner : IRefiner<TimeRange<IpHostContextWithTags>, HostContext>
    {
        private static InternetServiceTag[] EmptyTags = Array.Empty<InternetServiceTag>();

        private readonly ILogger? _logger;
        private readonly PerformanceCounters? _counters;

        /// <summary>
        /// Creates a new instance of the IpHostContextSimplifier class.
        /// </summary>
        public IpHostContextRefiner(PerformanceCounters? counters = null, ILogger? logger = null)
        {
            _logger = logger;
            _counters = counters;
        }

        /// <summary>
        /// Refines the given <see cref="TimeRange{IpHostContextWithTags}"/> value and returns a <see cref="HostContext"/> object.
        /// </summary>
        /// <param name="value">The value to be refined.</param>
        /// <returns>The refined <see cref="HostContext"/> object, or null if the refinement fails.</returns>
        public HostContext? Refine(TimeRange<IpHostContextWithTags> value)
        {
            if (value.Value == null || value.Value.HostAddress == null)
                return null;

            Dictionary<string, IpConnectionInfo> connections = new Dictionary<string, IpConnectionInfo>();
            LinkedList<ResolvedDomainInfo> domains = new LinkedList<ResolvedDomainInfo>();
            LinkedList<WebRequestInfo> webrequests = new LinkedList<WebRequestInfo>();
            LinkedList<TlsHandshakeInfo> tlshandshakes = new LinkedList<TlsHandshakeInfo>();
            try
            {
                var hostFlows = (value.Value.Flows) ?? Array.Empty<IpFlow>();
                foreach (var flow in hostFlows)
                {
                    switch (flow)
                    {
                        case DnsFlow dnsFlow:
                            if (!String.IsNullOrWhiteSpace(dnsFlow.ResponseData))
                                if (value.Value.HostAddress.Equals(flow.SourceAddress))
                                {
                                    domains.AddLast(new ResolvedDomainInfo(SafeString(dnsFlow.DestinationAddress), SafeString(dnsFlow.QuestionName), SafeString(dnsFlow.ResponseData), dnsFlow.ResponseCode));
                                }
                                else
                                {
                                    domains.AddLast(new ResolvedDomainInfo(SafeString(dnsFlow.SourceAddress), SafeString(dnsFlow.QuestionName), SafeString(dnsFlow.ResponseData), dnsFlow.ResponseCode));
                                }
                            break;
                        case HttpFlow httpFlow:
                            if (value.Value.HostAddress.Equals(flow.SourceAddress))
                            {
                                webrequests.AddLast(new WebRequestInfo(SafeString(httpFlow.DestinationAddress), SafeString(httpFlow.Hostname), httpFlow.DestinationPort, String.Empty, EmptyTags, SafeString(httpFlow.Method), SafeString(httpFlow.Hostname) + SafeString(httpFlow.Url)));
                            }
                            break;
                        case TlsFlow tlsFlow:
                            if (value.Value.HostAddress.Equals(flow.SourceAddress))
                            {
                                tlshandshakes.AddLast(new TlsHandshakeInfo(SafeString(tlsFlow.DestinationAddress), String.Empty, tlsFlow.DestinationPort, String.Empty, EmptyTags, SafeString(tlsFlow.ApplicationLayerProtocolNegotiation), SafeString(tlsFlow.ServerNameIndication), SafeString(tlsFlow.JA3Fingerprint), SafeString(tlsFlow.IssuerCommonName), SafeString(tlsFlow.SubjectCommonName), SafeString(tlsFlow.SubjectOrganisationName), SafeString(tlsFlow.CipherSuites), SafeString(tlsFlow.EllipticCurves)));
                            }
                            break;
                    }

                    if (value.Value.HostAddress.Equals(flow.SourceAddress))
                    {
                        AddConnection(SafeString(flow.DestinationAddress), String.Empty, flow.DestinationPort, String.Empty, EmptyTags, flow.SentPackets, flow.SentOctets, flow.RecvPackets, flow.RecvOctets);
                    }
                    else
                    {
                        AddConnection(SafeString(flow.SourceAddress), String.Empty, flow.SourcePort, String.Empty, EmptyTags, flow.RecvPackets, flow.RecvOctets, flow.SentPackets, flow.SentOctets);
                    }
                }
                var hostContext = new HostContext
                {
                    Start = value.StartTime,
                    End = value.EndTime,
                    Key = value.Value.HostAddress.ToString(),
                    Connections = connections.Values.ToArray(),
                    ResolvedDomains = domains.Distinct().ToArray(),
                    WebUrls = webrequests.Distinct().ToArray(),
                    TlsHandshakes = tlshandshakes.Distinct().ToArray(),
                    Tags = new Dictionary<string, object>()
                };

                return hostContext;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"Error in refining context {value.Value.HostAddress}@{value.StartTime}-{value.EndTime}.");
                return null;
            }

            void AddConnection(string address, string empty1, ushort port, string application, InternetServiceTag[] emptyTags, int sentPackets, long sentOctets, int recvPackets, long recvOctets)
            {
                if (connections.TryGetValue($"{address}@{port}", out var connection))
                {
                    connection.PacketsSent += sentPackets;
                    connection.OctetsSent += sentOctets;
                    connection.PacketsRecv += recvPackets;
                    connection.OctetsRecv += recvOctets;
                    connection.Flows++;
                }
                else
                {
                    connections[$"{address}@{port}"] = new IpConnectionInfo(address, String.Empty, port, application, emptyTags, 1, sentPackets, sentOctets, recvPackets, recvOctets);
                }
            }

        }

        static string SafeString<T>(T? value)
        {
            return value?.ToString() ?? String.Empty;
        }
        public class PerformanceCounters
        {
            /// <summary>
            /// Gets or sets the number of inputs.
            /// </summary>
            public int InputCount;
            /// <summary>
            /// Gets or sets the number of outputs.
            /// </summary>
            public int OutputCount;
            /// <summary>
            /// Gets or sets the number of hosts connections.
            /// </summary>
            public int HostsConnections;
            /// <summary>
            /// The number of hosts with resolved domains.
            /// </summary>
            public int HostsResolvedDomains;
            /// <summary>
            /// Gets or sets the number of web URLs hosted by the IP host.
            /// </summary>
            public int HostsWebUrls;
            /// <summary>
            /// Gets or sets the number of TLS handshakes performed by the host.
            /// </summary>
            public int HostsTlsHandshakes;
            /// <summary>
            /// The number of errors encountered during the refining process.
            /// </summary>
            public int Errors;
        }
    }
}
