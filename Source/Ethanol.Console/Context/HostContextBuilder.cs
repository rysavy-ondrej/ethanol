using Ethanol.Catalogs;
using Ethanol.Streaming;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Hosting;
using Microsoft.StreamProcessing;
using System;
using System.IO;
using System.Linq;

namespace Ethanol.Console
{


    public record HostContext<T>
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


    public static class HostContextBuilder
    {
        private static readonly string NBAR_DNS = "DNS_TCP";
        private static readonly string NBAR_TLS = "SSL/TLS";
        private static readonly string NBAR_HTTPS = "HTTPS";
        private static readonly string NBAR_HTTP = "HTTP";
        public static IStreamable<Empty, HostContext<NetworkActivity>> BuildHostContext (this ContextBuilderCatalog _, IStreamable<Empty, IpfixRecord> source)
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
                return xy.Select(x=> GetNetworkActivity(x));
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e);
                throw;
            }
        }

        private static HostContext<NetworkActivity> GetNetworkActivity(HostFlows x)
        {
            return new HostContext<NetworkActivity> { HostKey = x.Host,
                Value =
                new NetworkActivity
                {
                    Http = x.Flows.Where(x => x.Nbar == NBAR_HTTP).Select(GetHttpRequest).ToArray(),
                    Https = x.Flows.Where(x => x.Nbar == NBAR_HTTPS).Select(GetHttpsConnection).ToArray(),
                    Dns = x.Flows.Where(x => x.Nbar == NBAR_DNS).Select(GetDnsResolution).ToArray(),
                    Tls = x.Flows.Where(x => x.Nbar == NBAR_TLS).Select(GetTlsData).ToArray()
            } };        
        }

        public static string GetHostAddress(IpfixRecord flow)
        {
            if (flow.Nbar == NBAR_DNS) return flow.SourceIpAddress;
            if (flow.Nbar == NBAR_TLS) return flow.SourceIpAddress;
            if (flow.Nbar == NBAR_HTTPS) return flow.SourceIpAddress;
            if (flow.Nbar == NBAR_HTTP) return flow.SourceIpAddress;
            return String.Empty;
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
