using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.Streaming;
using Microsoft.CodeAnalysis;
using Microsoft.StreamProcessing;
using NRules.Fluent.Dsl;
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
    [Plugin(PluginCategory.Builder, "HostContext", "Builds the context for IP hosts identified in the source IPFIX stream.")]
    public class HostContextBuilder : ContextBuilder<IpFlow, KeyValuePair<IPAddress, NetworkActivity>, ContextObject<IPAddress, HostContext>>
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
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
        internal static IObservableTransformer<IpFlow, object> Create(Configuration configuration)
        {
            return new HostContextBuilder(configuration.Window, configuration.Hop);
        }

        public override IStreamable<Microsoft.StreamProcessing.Empty, KeyValuePair<IPAddress, NetworkActivity>> BuildContext(IStreamable<Microsoft.StreamProcessing.Empty, IpFlow> source)
        {
            return BuildHostContext(source);
        }

        protected override ContextObject<IPAddress, HostContext> GetTarget(StreamEvent<KeyValuePair<IPAddress, NetworkActivity>> arg)
        {
            return new ContextObject<IPAddress, HostContext>
            {
                Id = Guid.NewGuid().ToString(),
                Window = WindowSpan.FromLong(arg.StartTime, arg.EndTime),
                Object = arg.Payload.Key,
                Context = new HostContext { Activity = arg.Payload.Value }
            };
        }

        static IStreamable<Microsoft.StreamProcessing.Empty, KeyValuePair<IPAddress, NetworkActivity>> BuildHostContext(IStreamable<Microsoft.StreamProcessing.Empty, IpFlow> flowStreamSource)
        {
            try
            {
                var hostBasedStream = flowStreamSource.Multicast(flowStream =>
                {
                    var upFlowsStream = flowStream.GroupApply(
                                                    obj => obj.SourceAddress,
                                                    group => group.Aggregate(aggregate => aggregate.CollectList(obj => obj)),
                                                    (key, value) => new { Host = key.Key, Flows = value });

                    var downFlowsStream = flowStream.GroupApply(
                                                    obj => obj.DestinationAddress,
                                                    group => group.Aggregate(aggregate => aggregate.CollectList(obj => obj)),
                                                    (key, value) => new { Host = key.Key, Flows = value });

                    var hostRelatedFlows = upFlowsStream.Join(downFlowsStream,
                    h => h.Host,
                    h => h.Host,
                        (up, down) => new HostConversations { Host = up.Host, Conversations = CreateConversations(up.Flows, down.Flows).ToArray() });

                    return hostRelatedFlows;
                });
                return hostBasedStream.Select(x => GetNetworkActivity(x));
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

        private static IEnumerable<Conversation> CreateConversations(IpFlow[] flows1, IpFlow[] flows2)
        {
            return flows1.Join(flows2, 
                k => k.FlowKey.GetHashCode(),
                k => k.FlowKey.GetReverseFlowKey().GetHashCode(), 
                (left,right) => new Conversation { UpFlow = left, DownFlow = right } );
        }

        /// <summary>
        /// Enriches the records in <paramref name="hostContextStream"/> with data from <paramref name="metadataStream"/> using host IP address as the join key.
        /// </summary>
        /// <param name="hostContextStream">The host context stream.</param>
        /// <param name="metadataStream">The metadata stream.</param>
        /// <returns>The stream with enriched host context.</returns>
        static IStreamable<Microsoft.StreamProcessing.Empty, Tuple<string, NetworkActivity, HostTag[]>> EnrichHostContext(IStreamable<Microsoft.StreamProcessing.Empty, KeyValuePair<string, NetworkActivity>> hostContextStream, IStreamable<Microsoft.StreamProcessing.Empty, HostTag> metadataStream)
        {
            try
            {
                return hostContextStream.Multicast(contextStream =>
                {
                    // collect metadata for the hosts for which we have context...
                    var metadata = contextStream
                    .Join(metadataStream, left => left.Key, right => right.HostAddress, (context, metadata) => new Tuple<string, HostTag>(context.Key, metadata))
                    .GroupApply(x => x.Item1, group => group.Aggregate(aggregate => aggregate.CollectList(x => x.Item2)),
                    (key, val) => new Tuple<string, HostTag[]>(key.Key, val));
                    // join host context and collected metadata
                    return contextStream.Join(metadata, x => x.Key, y => y.Item1, (x, y) => new Tuple<string, NetworkActivity, HostTag[]>(x.Key, x.Value, y.Item2));
                });
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Split the input flows to separate collection according to their types.
        /// </summary>
        /// <param name="flows"></param>
        /// <returns></returns>
        private static KeyValuePair<IPAddress, NetworkActivity> GetNetworkActivity(HostConversations flows)
        {
            return new KeyValuePair<IPAddress, NetworkActivity>
                (flows.Host, new NetworkActivity
                {
                    PlainWeb = flows.Conversations.Where(x => x.UpFlow is HttpFlow).Select(x => (x.UpFlow as HttpFlow)).Select(HttpRequest.Create).ToArray(),
                    Domains = flows.Conversations.Where(x => x.DownFlow is DnsFlow).Select(x => (x.DownFlow as DnsFlow)).Select(DnsResolution.Create).ToArray(),
                    Encrypted = flows.Conversations.Where(x => x.UpFlow is TlsFlow).Select(x => (x.UpFlow as TlsFlow)).Select(TlsConnection.Create).Where(x=> !String.IsNullOrEmpty(x.Version)).ToArray(),
                    RemoteHosts = flows.Conversations.Where(x=>x.ConversationKey.DestinationAddress != flows.Host).GroupBy(x => x.ConversationKey.DestinationAddress).Select(GetRemotehostStats).ToArray()
                });
        }
        private static RemoteHostConnections GetRemotehostStats(IGrouping<IPAddress, Conversation> x)
        {
            return new RemoteHostConnections
            {
                HostAddress = x.Key,
                Flows = x.Count(),
                Ports = x.Select(f=>f.ConversationKey.DestinationPort).Distinct().ToArray(),
                AverageConnectionTime = TimeSpan.FromMilliseconds(x.Average(t=> t.UpFlow.TimeDuration.TotalMilliseconds)),
                MinConnectionTime = TimeSpan.FromMilliseconds(x.Min(t => t.UpFlow.TimeDuration.TotalMilliseconds)),
                MaxConnectionTime = TimeSpan.FromMilliseconds(x.Max(t => t.UpFlow.TimeDuration.TotalMilliseconds)),
                SendPackets = x.Sum(p => p.UpFlow.RecvPackets),
                SendBytes = x.Sum(p => p.UpFlow.RecvOctets),
                RecvPackets = x.Sum(p => p.DownFlow.RecvPackets),
                RecvBytes = x.Sum(p => p.DownFlow.RecvOctets)
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
        public static IStreamable<Microsoft.StreamProcessing.Empty, KeyValuePair<IPAddress, NetworkActivity>> BuildHostContext(this ContextBuilderCatalog _, IStreamable<Microsoft.StreamProcessing.Empty, IpFlow> source, TimeSpan windowSize, TimeSpan windowHop)
        {
            var builder = new HostContextBuilder(windowSize, windowHop);
            return builder.BuildContext(source);
        }
    }
}
