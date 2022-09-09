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


    public record HostContext<T>(string HostKey, T Value);

    public record NetworkActivity(HttpRequest[] Http, HttpsConnection[] Https, DnsResolution[] Dns, TlsData[] Tls);

    public record HttpsConnection(FlowKey Flow, string DomainName);
    public record HttpRequest(FlowKey Flow, string Url, string Method, string Response);
    public record DnsResolution(FlowKey Flow, string DomainNane, string[] Addresses);
    public record TlsData(FlowKey Flow, string RequestHost, string TlsVersion, string JA3, string SNI, string CommonName);

    public record HostFlows(string Host, IpfixRecord[] Flows);

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
                                    (key, value) => new HostFlows(key.Key, value));

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
            return new HostContext<NetworkActivity>(x.Host,
                new NetworkActivity(
                    x.Flows.Where(x => x.Nbar == NBAR_HTTP).Select(GetHttpRequest).ToArray(),
                    x.Flows.Where(x => x.Nbar == NBAR_HTTPS).Select(GetHttpsConnection).ToArray(),
                    x.Flows.Where(x => x.Nbar == NBAR_DNS).Select(GetDnsResolution).ToArray(),
                    x.Flows.Where(x => x.Nbar == NBAR_TLS).Select(GetTlsData).ToArray()));
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
            return new HttpsConnection(record.FlowKey, record.HttpHost);
        }

        private static HttpRequest GetHttpRequest(IpfixRecord record)
        {
            return new HttpRequest(record.FlowKey, record.HttpHost + record.HttpUrl, record.HttpMethod, record.HttpResponse);
        }

        private static DnsResolution GetDnsResolution(IpfixRecord record)
        {
            return new DnsResolution(record.FlowKey, record.DnsQueryName, record.DnsResponseData?.Split(',') ?? Array.Empty<string>());
        }

        private static TlsData GetTlsData(IpfixRecord record)
        {
            return new TlsData(record.FlowKey, record.HttpHost, record.TlsClientVersion, record.TlsJa3, record.TlsServerName, record.TlsServerCommonName);
        }
    }
}
