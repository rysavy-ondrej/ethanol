using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Readers;
using Ethanol.Streaming;
using Microsoft.CodeAnalysis;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Builds the context for Ip hosts identified in the source IPFIX stream.
    /// </summary>
    [Plugin(PluginType.Builder, "HostContext", "Builds the context for IP hosts identified in the source IPFIX stream.")]
    public class HostContextBuilder : ContextBuilder<IpfixObject, KeyValuePair<string,NetworkActivity>, ContextObject<string, HostContext>>
    {
        public class Configuration
        {
            [YamlMember(Alias = "window", Description = "The time span of window.")]
            public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(60);
            [YamlMember(Alias = "hop", Description = "The time span of window hop.")]
            public TimeSpan Hop { get; set; } = TimeSpan.FromSeconds(30);
        }

        public HostContextBuilder(TimeSpan windowSize, TimeSpan windowHop) : base(new IpfixObservableStream(windowSize, windowHop))
        {
        }

        [PluginCreate]
        internal static IContextBuilder<IpfixObject, object> Create(Configuration configuration)
        {
            return new HostContextBuilder(configuration.Window, configuration.Hop);
        }

        public override IStreamable<Empty, KeyValuePair<string,NetworkActivity>> BuildContext(IStreamable<Empty, IpfixObject> source)
        {
            return BuildHostContext(source);
        }

        protected override ContextObject<string, HostContext> GetTarget(StreamEvent<KeyValuePair<string,NetworkActivity>> arg)
        {
            return new ContextObject<string, HostContext>
            {
                Id =      Guid.NewGuid().ToString(),
                Window =  WindowSpan.FromLong(arg.StartTime, arg.EndTime),
                Object =  arg.Payload.Key,
                Context = new HostContext { Activity = arg.Payload.Value }
            };
        }

        private static readonly string NBAR_DNS = ApplicationProtocols.DNS.ToString();
        private static readonly string NBAR_TLS = ApplicationProtocols.SSL.ToString();
        private static readonly string NBAR_HTTPS = ApplicationProtocols.HTTPS.ToString();
        private static readonly string NBAR_HTTP = ApplicationProtocols.HTTP.ToString();

        static IStreamable<Empty, KeyValuePair<string, NetworkActivity>> BuildHostContext(IStreamable<Empty, IpfixObject> flowStreamSource)
        {
            try
            {
                var xy = flowStreamSource.Multicast(flowStream =>
                {
                    var hostAddressStream = flowStream.Select(r => r.SourceIpAddress).Distinct();
                    var hostRelatedFlows = hostAddressStream
                        .Join(flowStream, hostIp => hostIp, flow => GetHostAddress(flow), (host, flow) => new { Host = host, Flow = flow })
                        .GroupApply(
                                    obj => obj.Host,
                                    group => group.Aggregate(aggregate => aggregate.CollectList(obj => obj.Flow)),
                                    (key, value) => new HostFlows { Host = key.Key, Flows = value });

                    return hostRelatedFlows;
                });
                return xy.Select(x => GetNetworkActivity(x));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                throw;
            }
        }
        /// <summary>
        /// Enriches the records in <paramref name="hostContextStream"/> with data from <paramref name="metadataStream"/> using host IP address as the join key.
        /// </summary>
        /// <param name="hostContextStream">The host context stream.</param>
        /// <param name="metadataStream">The metadata stream.</param>
        /// <returns>The stream with enriched host context.</returns>
        static IStreamable<Empty, Tuple<string, NetworkActivity, HostMetadata[]>> EnrichHostContext(IStreamable<Empty, KeyValuePair<string, NetworkActivity>> hostContextStream, IStreamable<Empty, HostMetadata> metadataStream)
        {
            try
            {
                return hostContextStream.Multicast(contextStream =>
                {
                    // collect metadata for the hosts for which we have context...
                    var metadata = contextStream
                    .Join(metadataStream, left => left.Key, right => right.HostAddress, (context, metadata) => new Tuple<string, HostMetadata>(context.Key, metadata))
                    .GroupApply(x => x.Item1, group => group.Aggregate(aggregate => aggregate.CollectList(x => x.Item2)),
                    (key, val) => new Tuple<string, HostMetadata[]>(key.Key, val));
                    // join host context and collected metadata
                    return contextStream.Join(metadata, x => x.Key, y => y.Item1, (x, y) => new Tuple<string, NetworkActivity, HostMetadata[]>(x.Key, x.Value, y.Item2));
                });
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                throw;
            }
        }

        private static KeyValuePair<string,NetworkActivity> GetNetworkActivity(HostFlows x)
        {
            return new KeyValuePair<string, NetworkActivity>
                (x.Host, new NetworkActivity
                {
                    Http = x.Flows.Where(x => x.AppProtoName == NBAR_HTTP).Select(GetHttpRequest).ToArray(),
                    Https = x.Flows.Where(x => x.AppProtoName == NBAR_HTTPS).Select(GetHttpsConnection).ToArray(),
                    Dns = x.Flows.Where(x => x.AppProtoName == NBAR_DNS).Select(GetDnsResolution).ToArray(),
                    Tls = x.Flows.Where(x => x.AppProtoName == NBAR_TLS).Select(GetTlsData).ToArray()
                });
        }

        /// <summary>
        /// Gets the host address from the supported <paramref name="flow"/>. 
        /// The flow should be NBAR annotated as DNS, TLS, HTTP or HTTPS.
        /// <para/>
        /// Note that this method needs to be public because it is used from stream pipeline.
        /// </summary>
        /// <param name="flow">The IPFIX flow object.</param>
        /// <returns>String representing the IP address or empty string if the flow </returns>
        public static string GetHostAddress(IpfixObject flow)
        {
            if (flow.AppProtoName == NBAR_DNS) return flow.SourceIpAddress;
            if (flow.AppProtoName == NBAR_TLS) return flow.SourceIpAddress;
            if (flow.AppProtoName == NBAR_HTTPS) return flow.SourceIpAddress;
            if (flow.AppProtoName == NBAR_HTTP) return flow.SourceIpAddress;
            return string.Empty;
        }

        private static HttpsConnection GetHttpsConnection(IpfixObject record)
        {
            return new HttpsConnection { Flow = record.FlowKey, DomainName = record.HttpHost };
        }

        private static HttpRequest GetHttpRequest(IpfixObject record)
        {
            return new HttpRequest { Flow = record.FlowKey, Url = record.HttpHost + record.HttpUrl, Method = record.HttpMethod, Response = record.HttpResponse };
        }

        private static DnsResolution GetDnsResolution(IpfixObject record)
        {
            return new DnsResolution { Flow = record.FlowKey, DomainNane = record.DnsQueryName, Addresses = record.DnsResponseData?.Split(',') ?? Array.Empty<string>() };
        }

        private static TlsData GetTlsData(IpfixObject record)
        {
            return new TlsData { Flow = record.FlowKey, RequestHost = record.HttpHost, TlsVersion = record.TlsVersion, JA3 = record.TlsJa3, SNI = record.TlsServerName, CommonName = record.TlsServerCommonName };
        }
    }
    static class IpHostContextBuilderCatalogueExtensions
    {
        /// <summary>
        /// Creates a TLS facts related to the given flow.
        /// </summary>
        /// <param name="source">Flow stream.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>A stream of flows with their contexts.</returns>
        public static IStreamable<Empty, KeyValuePair<string,NetworkActivity>> BuildHostContext(this ContextBuilderCatalog _, IStreamable<Empty, IpfixObject> source, TimeSpan windowSize, TimeSpan windowHop)
        {
            var builder = new HostContextBuilder(windowSize, windowHop);
            return builder.BuildContext(source);
        }
    }
}
