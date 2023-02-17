using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.Streaming;
using Microsoft.CodeAnalysis;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Builds the context for Ip hosts identified in the source IPFIX stream.
    /// </summary>
    [Plugin(PluginType.Builder, "HostContext", "Builds the context for IP hosts identified in the source IPFIX stream.")]
    public class HostContextBuilder : ContextBuilder<IpFlow, KeyValuePair<string, NetworkActivity>, ContextObject<string, HostContext>>
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
        internal static IContextBuilder<IpFlow, object> Create(Configuration configuration)
        {
            return new HostContextBuilder(configuration.Window, configuration.Hop);
        }

        public override IStreamable<Empty, KeyValuePair<string, NetworkActivity>> BuildContext(IStreamable<Empty, IpFlow> source)
        {
            return BuildHostContext(source);
        }

        protected override ContextObject<string, HostContext> GetTarget(StreamEvent<KeyValuePair<string, NetworkActivity>> arg)
        {
            return new ContextObject<string, HostContext>
            {
                Id = Guid.NewGuid().ToString(),
                Window = WindowSpan.FromLong(arg.StartTime, arg.EndTime),
                Object = arg.Payload.Key,
                Context = new HostContext { Activity = arg.Payload.Value }
            };
        }

        static IStreamable<Empty, KeyValuePair<string, NetworkActivity>> BuildHostContext(IStreamable<Empty, IpFlow> flowStreamSource)
        {
            try
            {
                var xy = flowStreamSource.Multicast(flowStream =>
                {
                    var hostAddressStream = flowStream.Select(r => r.SourceAddress.ToString()).Distinct();
                    var hostRelatedFlows = hostAddressStream
                        .Join(flowStream, hostIp => hostIp, flow => GetHostAddress(flow).ToString(), (host, flow) => new { Host = host, Flow = flow })
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

        /// <summary>
        /// Split the input flows to separate collection according to their types.
        /// </summary>
        /// <param name="flows"></param>
        /// <returns></returns>
        private static KeyValuePair<string, NetworkActivity> GetNetworkActivity(HostFlows flows)
        {
            return new KeyValuePair<string, NetworkActivity>
                (flows.Host, new NetworkActivity
                {
                    PlainWeb = flows.Flows.Where(x => x is HttpFlow).Select(x => GetHttpRequest(x as HttpFlow)).Distinct().ToArray(),
                    Domains = flows.Flows.Where(x => x is DnsFlow).Select(x => GetDnsResolution(x as DnsFlow)).Distinct().ToArray(),
                    Encrypted = flows.Flows.Where(x => x is TlsFlow).Select(x => GetTlsData(x as TlsFlow)).Distinct().ToArray()
                });
        }

        /// <summary>
        /// Gets the host address from the supported <paramref name="flow"/>. 
        /// <para/>
        /// Note that this method needs to be public because it is used from stream pipeline.
        /// </summary>
        /// <param name="flow">The flow object.</param>
        /// <returns>IP Addess of the flow. It gets source ip address except for DNS flows in which case it returns destinaiton address.</returns>
        public static IPAddress GetHostAddress(InternetFlow flow) => flow switch
        {
            DnsFlow d => d.DestinationAddress,
            _ => flow.SourceAddress
        };

        private static HttpRequest GetHttpRequest(HttpFlow record)
        {
            return new HttpRequest { Flow = record.FlowKey, Url = record.Hostname + record.URL, Method = record.Method, Response = record.ResultCode };
        }

        private static DnsResolution GetDnsResolution(DnsFlow record)
        {
            return new DnsResolution { Flow = record.FlowKey, DomainNane = record.QuestionName, Addresses = record.ResponseData?.Split(',') ?? Array.Empty<string>() };
        }

        private static TlsConnection GetTlsData(TlsFlow record)
        {
            return new TlsConnection
            {
                Flow = record.FlowKey,
                Version = record.ServerVersion,
                JA3 = record.JA3Fingerprint,
                ServerNameIndication = record.ServerNameIndication,
                SubjectCommonName = record.SubjectCommonName,
                ApplicationLayerProtocolNegotiation = record.ApplicationLayerProtocolNegotiation
            };
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
        public static IStreamable<Empty, KeyValuePair<string, NetworkActivity>> BuildHostContext(this ContextBuilderCatalog _, IStreamable<Empty, IpFlow> source, TimeSpan windowSize, TimeSpan windowHop)
        {
            var builder = new HostContextBuilder(windowSize, windowHop);
            return builder.BuildContext(source);
        }
    }
}
