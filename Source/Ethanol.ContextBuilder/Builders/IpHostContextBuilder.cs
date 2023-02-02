using Elastic.Clients.Elasticsearch.Fluent;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.Streaming;
using Microsoft.StreamProcessing;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using static System.Net.WebRequestMethods;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Represents a host context.
    /// </summary>
    /// <typeparam name="T">The content type of the context.</typeparam>
    public record HostContext<T>
    {
        public string HostKey { get; set; }

        public WindowSpan Window { get; set; }
        public T Value { get; set; }
    }
    /// <summary>
    /// Represents an internal representation of a host context.
    /// </summary>
    /// <typeparam name="T">The content type of the context.</typeparam>
    public record InternalHostContext<T>
    {
        public string HostKey { get; set; }
        public T Value { get; set; }
    }
    /// <summary>
    /// Represents host-related network activity/flows, which is a part of the host context.
    /// </summary>
    public record NetworkActivity
    {
        public HttpRequest[] Http { get; set; }
        public HttpsConnection[] Https { get; set; }
        public DnsResolution[] Dns { get; set; }
        public TlsData[] Tls { get; set; }
    }
    /// <summary>
    /// Represents selected information from HTTPS connection.
    /// </summary>
    public record HttpsConnection
    {
        public IpfixKey Flow { get; set; }
        public string DomainName { get; set; }

    }
    /// <summary>
    /// Represents selected information from HTTP connection. 
    /// </summary>
    public record HttpRequest
    {
        public IpfixKey Flow { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public string Response { get; set; }
    }
    /// <summary>
    ///  Represents selected information from DNS connection.
    /// </summary>
    public record DnsResolution
    {
        public IpfixKey Flow { get; set; }
        public string DomainNane { get; set; }
        public string[] Addresses { get; set; }
    }
    /// <summary>
    ///  Represents selected information from TLS connection.
    /// </summary>
    public record TlsData
    {
        public IpfixKey Flow { get; set; }
        public string RequestHost { get; set; }
        public string TlsVersion { get; set; }
        public string JA3 { get; set; }
        public string SNI { get; set; }
        public string CommonName { get; set; }
    }
    /// <summary>
    ///  Represents a collection of flows related to the host.
    /// </summary>
    public record HostFlows
    {
        public string Host { get; set; }
        public IpfixObject[] Flows { get; set; }
    }
    /// <summary>
    /// Builds the context for Ip hosts identified in the source IPFIX stream.
    /// </summary>
    [Plugin(PluginType.Builder, "HostContext", "Builds the context for Ip hosts identified in the source IPFIX stream.")]
    public class IpHostContextBuilder : ContextBuilder<IpfixObject, InternalHostContext<NetworkActivity>, HostContext<NetworkActivity>>
    {
        public class Configuration
        {
            [YamlMember(Alias = "window", Description = "The time span of window.")]
            public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(60);
            [YamlMember(Alias = "hop", Description = "The time span of window hop.")]
            public TimeSpan Hop { get; set; } = TimeSpan.FromSeconds(30);
        }

        public IpHostContextBuilder(TimeSpan windowSize, TimeSpan windowHop) : base(new IpfixObservableStream(windowSize, windowHop))
        {
        }

        [PluginCreate]
        internal static IContextBuilder<IpfixObject, object> Create(Configuration configuration)
        {
            return new IpHostContextBuilder(configuration.Window, configuration.Hop);
        }

        public override IStreamable<Empty, InternalHostContext<NetworkActivity>> BuildContext(IStreamable<Empty, IpfixObject> source)
        {
            return BuildHostContext(source);
        }

        protected override HostContext<NetworkActivity> GetTarget(StreamEvent<InternalHostContext<NetworkActivity>> arg)
        {
            return new HostContext<NetworkActivity> { HostKey = arg.Payload.HostKey, Window = WindowSpan.FromLong(arg.StartTime, arg.EndTime), Value = arg.Payload.Value };
        }

        private static readonly string NBAR_DNS = "DNS_TCP";
        private static readonly string NBAR_TLS = "SSL/TLS";
        private static readonly string NBAR_HTTPS = "HTTPS";
        private static readonly string NBAR_HTTP = "HTTP";
        
        static IStreamable<Empty, InternalHostContext<NetworkActivity>> BuildHostContext( IStreamable<Empty, IpfixObject> source)
        {
            try
            {
                var xy = source.Multicast(flowStream =>
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

        private static InternalHostContext<NetworkActivity> GetNetworkActivity(HostFlows x)
        {
            return new InternalHostContext<NetworkActivity>
            {
                HostKey = x.Host,
                Value =
                new NetworkActivity
                {
                    Http = x.Flows.Where(x => x.Nbar == NBAR_HTTP).Select(GetHttpRequest).ToArray(),
                    Https = x.Flows.Where(x => x.Nbar == NBAR_HTTPS).Select(GetHttpsConnection).ToArray(),
                    Dns = x.Flows.Where(x => x.Nbar == NBAR_DNS).Select(GetDnsResolution).ToArray(),
                    Tls = x.Flows.Where(x => x.Nbar == NBAR_TLS).Select(GetTlsData).ToArray()
                }
            };
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
            if (flow.Nbar == NBAR_DNS) return flow.SourceIpAddress;
            if (flow.Nbar == NBAR_TLS) return flow.SourceIpAddress;
            if (flow.Nbar == NBAR_HTTPS) return flow.SourceIpAddress;
            if (flow.Nbar == NBAR_HTTP) return flow.SourceIpAddress;
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
        public static IStreamable<Empty, InternalHostContext<NetworkActivity>> BuildHostContext(this ContextBuilderCatalog _, IStreamable<Empty, IpfixObject> source, TimeSpan windowSize, TimeSpan windowHop)
        {
            var builder = new IpHostContextBuilder(windowSize, windowHop);
            return builder.BuildContext(source);
        }
    }
}
