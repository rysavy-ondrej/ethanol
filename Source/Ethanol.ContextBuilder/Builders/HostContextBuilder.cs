using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
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
    [Plugin(PluginType.Builder, "HostContext", "Builds the context for IP hosts identified in the source IPFIX stream.")]
    public class HostContextBuilder : ContextBuilder<IpFlow, KeyValuePair<IPAddress, NetworkActivity>, ContextObject<IPAddress, HostContext>>
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

        public override IStreamable<Empty, KeyValuePair<IPAddress, NetworkActivity>> BuildContext(IStreamable<Empty, IpFlow> source)
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

        static IStreamable<Empty, KeyValuePair<IPAddress, NetworkActivity>> BuildHostContext(IStreamable<Empty, IpFlow> flowStreamSource)
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
                Console.Error.WriteLine(e);
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
        private static KeyValuePair<IPAddress, NetworkActivity> GetNetworkActivity(HostConversations flows)
        {
            return new KeyValuePair<IPAddress, NetworkActivity>
                (flows.Host, new NetworkActivity
                {
                    PlainWeb = flows.Conversations.Where(x => x.UpFlow is HttpFlow).Select(x => (x.UpFlow as HttpFlow)).Select(GetHttpRequest).ToArray(),
                    Domains = flows.Conversations.Where(x => x.DownFlow is DnsFlow).Select(x => (x.DownFlow as DnsFlow)).Select(GetDnsResolution).ToArray(),
                    Encrypted = flows.Conversations.Where(x => x.UpFlow is TlsFlow).Select(x => (x.UpFlow as TlsFlow)).Select(GetTlsConnection).Where(x=> !String.IsNullOrEmpty(x.Version)).ToArray(),
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
                SendPackets = x.Sum(p => p.UpFlow.PacketDeltaCount),
                SendBytes = x.Sum(p => p.UpFlow.OctetDeltaCount),
                RecvPackets = x.Sum(p => p.DownFlow.PacketDeltaCount),
                RecvBytes = x.Sum(p => p.DownFlow.OctetDeltaCount)
            };
        }
    
        private static HttpRequest GetHttpRequest(HttpFlow record)
        {
            return new HttpRequest { 
                FlowKey = record.FlowKey, 
                Url = record.Hostname + record.Url, 
                Method = record.Method, 
                Response = record.ResultCode 
            };
        }

        private static DnsResolution GetDnsResolution(DnsFlow record)
        {
            return new DnsResolution { 
                FlowKey = record.FlowKey,
                QueryClass = record.QuestionClass,
                QueryType = record.QuestionType,
                QuestionName= record.QuestionName,
                ResponseCode= record.ResponseCode,
                AnswerRecord = record.ResponseData?.Split(',') ?? Array.Empty<string>(),
                ResponseTTL = record.ResponseTTL
            };
        }

        private static TlsConnection GetTlsConnection(TlsFlow record)
        {
            return new TlsConnection
            {
                FlowKey = record.FlowKey,
                Version = record.ServerVersion,
                JA3 = record.JA3Fingerprint,
                CipherSuite = record.ServerCipherSuite,
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
        public static IStreamable<Empty, KeyValuePair<IPAddress, NetworkActivity>> BuildHostContext(this ContextBuilderCatalog _, IStreamable<Empty, IpFlow> source, TimeSpan windowSize, TimeSpan windowHop)
        {
            var builder = new HostContextBuilder(windowSize, windowHop);
            return builder.BuildContext(source);
        }
    }
}
