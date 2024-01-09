using Ethanol.ContextBuilder.Enrichers.TagObjects;
using Ethanol.DataObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;

namespace Ethanol.ContextBuilder.Polishers
{
    public class IpHostContextRefiner : IRefiner<TimeRange<IpHostContextWithTags>, HostContext>
    {

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


        public HostContext? Refine(TimeRange<IpHostContextWithTags> value)
        {
            if (_counters != null) _counters.InputCount++;

            if (value.Value == null || value.Value.HostAddress == null)
                return null;

            try
            {
                var hostFlows = (value.Value.Flows) ?? Array.Empty<IpFlow>();
                var domainResolver = CreateDomainResolver(hostFlows);

                var tags = value.Value.Tags ?? Array.Empty<TagObject>();
                var processResolver = CreateProcessResolver(tags);
                var serviceResolver = CreateServiceResolver(tags);

                string ResolveDomain(string address)
                {
                    return domainResolver.TryResolve(address, x => x.QueryString, out var result) ? result : String.Empty;
                }
                string ResolveProcessName(FlowKey flowKey)
                {
                    return processResolver.TryResolve($"{flowKey.SourceAddress}:{flowKey.SourcePort}-{flowKey.DestinationAddress}:{flowKey.DestinationPort}", x => x.ProcessName, out var result) ? result : String.Empty;
                }
                InternetServiceTag[] ResolveServices(string destinationAddress)
                {
                    return serviceResolver.TryResolve(destinationAddress, out var result) ? result.Item2 : Array.Empty<InternetServiceTag>();
                }

                var connections = AggregateHostConnections(value.Value.HostAddress, hostFlows, ResolveDomain, ResolveServices);

                var domains = CollectDomains(value.Value.HostAddress, hostFlows.SelectFlows<DnsFlow>());

                var webUrls = CollectUrls(value.Value.HostAddress, hostFlows.SelectFlows<HttpFlow>(), ResolveDomain, ResolveProcessName, ResolveServices);

                var handshakes = CollectTls(value.Value.HostAddress, hostFlows.SelectFlows<TlsFlow>(), ResolveDomain, ResolveProcessName, ResolveServices);

                var hostKey = SafeString(value.Value.HostAddress);

                
                var hostContext = new HostContext
                {
                    Start = value.StartTime,
                    End = value.EndTime,
                    Key = hostKey,
                    Connections = connections.ToArray(),
                    ResolvedDomains = domains.Distinct().ToArray(),
                    WebUrls = webUrls.Distinct().ToArray(),
                    TlsHandshakes = handshakes.Distinct().ToArray(),
                    Tags = new Dictionary<string, object>()
                };

                if (_counters != null)
                {
                    _counters.HostsConnections += hostContext.Connections.Length;
                    _counters.HostsResolvedDomains += hostContext.ResolvedDomains.Length;
                    _counters.HostsWebUrls += hostContext.WebUrls.Length;
                    _counters.HostsTlsHandshakes += hostContext.TlsHandshakes.Length;
                    _counters.OutputCount++;
                }
                return hostContext;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error in context polishing for event {0}.", value);
                if (_counters != null) _counters.Errors++;
                return null;
            }
        }

        /// <summary>
        /// Collects TLS handshake information for a given host address.
        /// </summary>
        /// <param name="hostAddress">The IP address of the host.</param>
        /// <param name="enumerable">The collection of TLS flows.</param>
        /// <param name="resolveDomain">A function to resolve the domain name from an IP address.</param>
        /// <param name="resolveProcessName">A function to resolve the process name from a flow key.</param>
        /// <param name="resolveServices">A function to resolve the internet service tags from a domain name.</param>
        /// <returns>An enumerable collection of TlsHandshakeInfo objects.</returns>
        private IEnumerable<TlsHandshakeInfo> CollectTls(IPAddress hostAddress, IEnumerable<TlsFlow> enumerable, Func<string, string> resolveDomain, Func<FlowKey, string> resolveProcessName, Func<string, InternetServiceTag[]> resolveServices)
        {
            var hostFlows = enumerable.Where(x => hostAddress.Equals(x.SourceAddress));
            foreach (var x in hostFlows)
            {
                yield return new TlsHandshakeInfo(
                    SafeString(x.DestinationAddress),
                    resolveDomain(SafeString(x.DestinationAddress)),
                    x.DestinationPort,
                    resolveProcessName(x.FlowKey),
                    resolveServices(SafeString(x.DestinationAddress)),
                    SafeString(x.ApplicationLayerProtocolNegotiation),
                    SafeString(x.ServerNameIndication),
                    SafeString(x.JA3Fingerprint),
                    SafeString(x.IssuerCommonName),
                    SafeString(x.SubjectCommonName),
                    SafeString(x.SubjectOrganisationName),
                    SafeString(x.CipherSuites),
                    SafeString(x.EllipticCurves));
            }
        }

        /// <summary>
        /// Collects the URLs from the given HTTP flows that match the specified host address.
        /// </summary>
        /// <param name="hostAddress">The IP address of the host.</param>
        /// <param name="httpFlows">The collection of HTTP flows.</param>
        /// <param name="resolveDomain">A function to resolve the domain name from the destination address.</param>
        /// <param name="resolveProcessName">A function to resolve the process name from the flow key.</param>
        /// <param name="resolveServices">A function to resolve the internet service tags from the destination address.</param>
        /// <returns>An enumerable collection of WebRequestInfo objects representing the collected URLs.</returns>
        private IEnumerable<WebRequestInfo> CollectUrls(IPAddress hostAddress, IEnumerable<HttpFlow> httpFlows, Func<string, string> resolveDomain, Func<FlowKey, string> resolveProcessName, Func<string, InternetServiceTag[]> resolveServices)
        {
            var requests = httpFlows.Where(x => hostAddress.Equals(x.SourceAddress));
            foreach (var request in requests)
            {
                yield return new WebRequestInfo(SafeString(request.DestinationAddress), resolveDomain(SafeString(request.DestinationAddress)), request.DestinationPort, resolveProcessName(request.FlowKey), resolveServices(SafeString(request.DestinationAddress)), SafeString(request.Method), SafeString(request.Hostname) + SafeString(request.Url));
            }
        }

        /// <summary>
        /// Collects distinct domains based on the provided host address, host flows, and domain resolution function.
        /// </summary>
        /// <param name="hostAddress">The IP address of the host.</param>
        /// <param name="hostFlows">The array of IP flows associated with the host.</param>
        /// <param name="resolveDomain">The function used to resolve a domain from a string.</param>
        /// <returns>An enumerable collection of resolved domain information.</returns>
        private IEnumerable<ResolvedDomainInfo> CollectDomains(IPAddress hostAddress, IEnumerable<DnsFlow> dnsFlows)
        {
            /* This is not working as we need to deal with both unidirectional and bidirestiobal flows!
            var questions = dnsFlows.Where(x => hostAddress.Equals(x.SourceAddress)).ToArray();
            var answers = dnsFlows.Where(x => hostAddress.Equals(x.DestinationAddress)).ToArray();
            foreach (var answer in answers)
            {
                yield return new ResolvedDomainInfo(SafeString(answer.SourceAddress), SafeString(answer.QuestionName), SafeString(answer.ResponseData), answer.ResponseCode);
            }
            */

            string GetOtherAddress(DnsFlow flow)
            {
                return hostAddress.Equals(flow.SourceAddress) ? SafeString(flow.DestinationAddress) : SafeString(flow.SourceAddress);
            }

            ResolvedDomainInfo GetDomainInfo(DnsFlow flow)
            {
                return new ResolvedDomainInfo(GetOtherAddress(flow), SafeString(flow.QuestionName), SafeString(flow.ResponseData), flow.ResponseCode);
            }

            IEnumerable<ResolvedDomainInfo> RemoveQueries(IEnumerable<ResolvedDomainInfo> queries)
            {
                var unique = queries.Distinct();
                if (unique.Count() == 1)
                {
                    return unique;
                }
                else
                {
                    return unique.Where(x => !String.IsNullOrEmpty(x.ResponseData));
                }
            }

            return dnsFlows
                .Select(dns => GetDomainInfo(dns))
                .GroupBy(dns => dns.QueryString)
                .SelectMany(g => RemoveQueries(g));
        }

        /// <summary>
        /// Creates a service resolver based on the provided tags.
        /// </summary>
        /// <param name="tags">The tags used to create the service resolver.</param>
        /// <returns>The created service resolver.</returns>
        private Resolver<string, (string, InternetServiceTag[])> CreateServiceResolver(IEnumerable<TagObject> tags)
        {
            var netifyTags = tags.Where(x => x.Type == "NetifyIp").GroupBy(g => g.Key ?? String.Empty, (k, v) => (k, v.Select(x => new InternetServiceTag(x.Value ?? String.Empty, (float)x.Reliability)).ToArray()));
            var serviceResolver = new Resolver<string, (string, InternetServiceTag[])>(netifyTags, t => t.Item1);
            return serviceResolver;
        }

        /// <summary>
        /// Creates a resolver for mapping TCP flow tags to process identifiers.
        /// </summary>
        /// <param name="tags">The collection of tags to be used for resolving.</param>
        /// <returns>A resolver object that maps TCP flow tags to process identifiers.</returns>
        private Resolver<string, TcpFlowTag> CreateProcessResolver(IEnumerable<TagObject> tags)
        {
#pragma warning disable CS8603 // Possible null reference return.          
            var flowTags = tags.Where(x => x.Type == nameof(TcpFlowTag)).Select<TagObject, TcpFlowTag>(x => x.GetDetailsAs<TcpFlowTag>()).Where(t => t != null).Select(t => t!).ToArray();
#pragma warning restore CS8603 // Possible null reference return.

            var processResolver = new Resolver<string, TcpFlowTag>(flowTags, flowTag => $"{flowTag.LocalAddress}:{flowTag.LocalPort}-{flowTag.RemoteAddress}:{flowTag.RemotePort}");
            return processResolver;
        }

        /// <summary>
        /// Creates a domain resolver based on the provided host flows.
        /// </summary>
        /// <param name="hostFlows">The host flows to create the resolver from.</param>
        /// <returns>The created domain resolver.</returns>
        private Resolver<string, IpDomainMap> CreateDomainResolver(IEnumerable<IpFlow> hostFlows)
        {
            var ipdomains = hostFlows.SelectFlows<DnsFlow>()
                .Where(d => !String.IsNullOrEmpty(d.ResponseData) && d.ResponseCode == DnsResponseCode.NoError && (d.ResponseType == DnsRecordType.A || d.ResponseType == DnsRecordType.AAAA))
                .Select(x => new IpDomainMap(SafeString(x.QuestionName), SafeString(x.ResponseData)))
                .ToArray() ?? Array.Empty<IpDomainMap>();

            return new Resolver<string, IpDomainMap>(ipdomains, d => SafeString(d.ResponseData));
        }
        record IpDomainMap(string QueryString, string ResponseData);

        /// <summary>
        /// Converts a nullable value to its string representation, or returns an empty string if the value is null.
        /// </summary>
        /// <typeparam name="T">The type of the nullable value.</typeparam>
        /// <param name="value">The nullable value to convert.</param>
        /// <returns>The string representation of the nullable value, or an empty string if the value is null.</returns>
        string SafeString<T>(T? value)
        {
            return value?.ToString() ?? String.Empty;
        }

        /// <summary>
        /// Aggregates the host connections based on the provided IP address, flows, and resolving functions.
        /// </summary>
        /// <param name="hostAddress">The IP address of the host.</param>
        /// <param name="flows">The collection of IP flows.</param>
        /// <param name="resolveDomain">The function to resolve the domain name from an IP address.</param>
        /// <param name="resolveServices">The function to resolve the internet service tags from an IP address.</param>
        /// <returns>The collection of aggregated host connections.</returns>
        private IEnumerable<IpConnectionInfo> AggregateHostConnections(IPAddress hostAddress, IEnumerable<IpFlow> flows, Func<string, string> resolveDomain, Func<string, InternetServiceTag[]> resolveServices)
        {
            // Perform processing on the input flows
            var connections = flows.Select(f => hostAddress.Equals(f.SourceAddress)
                ? new IpConnectionInfo(SafeString(f.DestinationAddress), String.Empty, f.DestinationPort, String.Empty, Array.Empty<InternetServiceTag>(), 1, f.SentPackets, f.SentOctets, f.RecvPackets, f.RecvOctets)
                : new IpConnectionInfo(SafeString(f.SourceAddress), String.Empty, f.SourcePort, String.Empty, Array.Empty<InternetServiceTag>(), 1, f.RecvPackets, f.RecvOctets, f.SentPackets, f.SentOctets)
            );
            // Return the resulting collection of initiated connections
            return connections.GroupBy(key => (key.RemoteHostAddress, key.RemotePort),
                        (key, val) =>
                        {
                            var address = key.RemoteHostAddress;
                            return new IpConnectionInfo(address, resolveDomain(address), key.RemotePort, String.Empty, resolveServices(address),
                                val.Count(), val.Sum(x => x.PacketsSent), val.Sum(x => x.OctetsSent),
                                val.Sum(x => x.PacketsRecv), val.Sum(x => x.OctetsRecv));
                        }
                    );
        }

        /// <summary>
        /// Provides a simple way to resolve a value by its key from a dictionary-like collection of values.
        /// </summary>
        /// <typeparam name="TKey">The type of the key used to index the values.</typeparam>
        /// <typeparam name="TValue">The type of the values to be stored and indexed.</typeparam>
        class Resolver<TKey, TValue> where TKey : notnull
        {
            Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
            /// <summary>
            /// Initializes a new instance of the <see cref="Resolver{TKey, TValue}"/> class with the specified collection of values and key selector function.
            /// </summary>
            /// <param name="values">An enumerable collection of <typeparamref name="TValue"/> objects to store in the resolver.</param>
            /// <param name="getKey">A function that takes a <typeparamref name="TValue"/> object and returns its corresponding key of type <typeparamref name="TKey"/>.</param>
            public Resolver(IEnumerable<TValue> values, Func<TValue, TKey> getKey)
            {
                foreach (var value in values)
                {
                    _dictionary[getKey(value)] = value;
                }
            }
            /// <summary>
            /// Resolves a value of type <typeparamref name="TValue"/> by the specified key.
            /// </summary>
            /// <param name="key">The key of type <typeparamref name="TKey"/> used to resolve the value.</param>
            /// <returns>The value of type <typeparamref name="TValue"/> that corresponds to the specified key, or the default value of <typeparamref name="TValue"/> if the key is not found.</returns>
            public bool TryResolve(TKey key, [NotNullWhen(true)] out TValue? value)
            {
                return _dictionary.TryGetValue(key, out value) && value != null;
            }
            /// <summary>
            /// Resolves a value of type <typeparamref name="TResult"/> by the specified key, using the specified selector function to transform the resolved value.
            /// </summary>
            /// <typeparam name="TResult">The type of the result to be returned.</typeparam>
            /// <param name="key">The key of type <typeparamref name="TKey"/> used to resolve the value.</param>
            /// <param name="select">A <see cref="Func{T, TResult}"/> that transforms the resolved value of type <typeparamref name="TValue"/> into a result of type <typeparamref name="TResult"/>.</param>
            /// <returns>The transformed value of type <typeparamref name="TResult"/> that corresponds to the specified key, or the default value of <typeparamref name="TResult"/> if the key is not found.</returns>
            public bool TryResolve<TResult>(TKey key, Func<TValue, TResult> select, [NotNullWhen(true)] out TResult? result)
            {
                if (_dictionary.TryGetValue(key, out var value))
                {
                    result = select(value);
                    return result != null;
                }
                else
                {
                    result = default;
                    return false;
                }
            }
        }

        public class PerformanceCounters
        {
            public int InputCount;
            public int OutputCount;
            public int HostsConnections;
            public int HostsResolvedDomains;
            public int HostsWebUrls;
            public int HostsTlsHandshakes;
            public int Errors;
        }
    }
}
