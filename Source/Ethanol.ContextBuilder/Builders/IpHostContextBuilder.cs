﻿using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Ethanol.Streaming;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Hosting;
using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ethanol.ContextBuilder.Builders
{

    public record HostContext<T>
    {
        public string HostKey { get; set; }

        public WindowSpan Window { get; set; }
        public T Value { get; set; }
    }

    public record InternalHostContext<T>
    {
        public string HostKey { get; set; }
        public T Value { get; set; }
    }
    public record NetworkActivity
    {
        public HttpRequest[] Http { get; set; }
        public HttpsConnection[] Https { get; set; }
        public DnsResolution[] Dns { get; set; }
        public TlsData[] Tls { get; set; }
    }

    public record HttpsConnection
    {
        public FlowKey Flow { get; set; }
        public string DomainName { get; set; }

    }

    public record HttpRequest
    {
        public FlowKey Flow { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public string Response { get; set; }
    }
    public record DnsResolution
    {
        public FlowKey Flow { get; set; }
        public string DomainNane { get; set; }
        public string[] Addresses { get; set; }
    }
    public record TlsData
    {
        public FlowKey Flow { get; set; }
        public string RequestHost { get; set; }
        public string TlsVersion { get; set; }
        public string JA3 { get; set; }
        public string SNI { get; set; }
        public string CommonName { get; set; }
    }

    public record HostFlows
    {
        public string Host { get; set; }
        public IpfixRecord[] Flows { get; set; }
    }

    public class IpHostContextBuilder : ContextBuilder<IpfixRecord, InternalHostContext<NetworkActivity>, HostContext<NetworkActivity>>
    {
        public IpHostContextBuilder(TimeSpan windowSize, TimeSpan windowHop) : base(new IpfixObservableStream(windowSize, windowHop))
        {
        }

        internal static IContextBuilder<IpfixRecord, object> Create(IReadOnlyDictionary<string, string> attributes)
        {
            if (!attributes.TryGetValue("window", out var windowSize)) windowSize = "00:01:00";
            if (!attributes.TryGetValue("hop", out var windowHop)) windowHop = "00:00:30";

            if (!TimeSpan.TryParse(windowSize, out var windowSizeTimeSpan)) windowSizeTimeSpan = TimeSpan.FromSeconds(60);
            if (!TimeSpan.TryParse(windowHop, out var windowHopTimeSpan)) windowHopTimeSpan = TimeSpan.FromSeconds(30);
            return new IpHostContextBuilder(windowSizeTimeSpan, windowHopTimeSpan);
        }

        protected override IStreamable<Empty, InternalHostContext<NetworkActivity>> BuildContext(IStreamable<Empty, IpfixRecord> source)
        {
            return _HostContextBuilder.BuildHostContext(source);
        }

        protected override HostContext<NetworkActivity> GetTarget(StreamEvent<InternalHostContext<NetworkActivity>> arg)
        {
            return new HostContext<NetworkActivity> { HostKey = arg.Payload.HostKey, Window = WindowSpan.FromLong(arg.StartTime, arg.EndTime), Value = arg.Payload.Value };
        }
    }

    public static class _HostContextBuilder
    {
        private static readonly string NBAR_DNS = "DNS_TCP";
        private static readonly string NBAR_TLS = "SSL/TLS";
        private static readonly string NBAR_HTTPS = "HTTPS";
        private static readonly string NBAR_HTTP = "HTTP";

        public static IStreamable<Empty, InternalHostContext<NetworkActivity>> BuildHostContext(this ContextBuilderCatalog _, IStreamable<Empty, IpfixRecord> source)
        {
            return BuildHostContext(source);
        }
        
        public static IStreamable<Empty, InternalHostContext<NetworkActivity>> BuildHostContext( IStreamable<Empty, IpfixRecord> source)
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

        public static string GetHostAddress(IpfixRecord flow)
        {
            if (flow.Nbar == NBAR_DNS) return flow.SourceIpAddress;
            if (flow.Nbar == NBAR_TLS) return flow.SourceIpAddress;
            if (flow.Nbar == NBAR_HTTPS) return flow.SourceIpAddress;
            if (flow.Nbar == NBAR_HTTP) return flow.SourceIpAddress;
            return string.Empty;
        }

        private static HttpsConnection GetHttpsConnection(IpfixRecord record)
        {
            return new HttpsConnection { Flow = record.FlowKey, DomainName = record.HttpHost };
        }

        private static HttpRequest GetHttpRequest(IpfixRecord record)
        {
            return new HttpRequest { Flow = record.FlowKey, Url = record.HttpHost + record.HttpUrl, Method = record.HttpMethod, Response = record.HttpResponse };
        }

        private static DnsResolution GetDnsResolution(IpfixRecord record)
        {
            return new DnsResolution { Flow = record.FlowKey, DomainNane = record.DnsQueryName, Addresses = record.DnsResponseData?.Split(',') ?? Array.Empty<string>() };
        }

        private static TlsData GetTlsData(IpfixRecord record)
        {
            return new TlsData { Flow = record.FlowKey, RequestHost = record.HttpHost, TlsVersion = record.TlsClientVersion, JA3 = record.TlsJa3, SNI = record.TlsServerName, CommonName = record.TlsServerCommonName };
        }
    }
}