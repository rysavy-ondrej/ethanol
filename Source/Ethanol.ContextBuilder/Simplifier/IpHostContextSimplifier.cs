using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Observable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;

namespace Ethanol.ContextBuilder.Simplifier
{

    public class IpHostContextSimplifier : IObservableTransformer<ObservableEvent<IpRichHostContext>, ObservableEvent<IpSimpleHostContext>>, IPipelineNode
    {
        // We use subject as the simplest way to implement the transformer.
        // For the production version, consider more performant implementation.
        private Subject<ObservableEvent<IpSimpleHostContext>> _subject;

        public PipelineNodeType NodeType => PipelineNodeType.Transformer;

        public IpHostContextSimplifier()
        {
            _subject = new Subject<ObservableEvent<IpSimpleHostContext>>();
        }

        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        public void OnNext(ObservableEvent<IpRichHostContext> value)
        {
            var domains = value.Payload.Flows.SelectFlows<DnsFlow>()
                .Select(x=>new ResolvedDomainInfo(x.SourceAddress, x.QuestionName, x.ResponseData, x.ResponseCode))
                .ToArray();
            
            var resolvedDictionary = GetDomainDictionary(domains);

            var flowTagDictionary = value.Payload.FlowTags.ToDictionary(x => $"{x.RemoteAddress}:{x.RemotePort}");


            string ResolveDomain(IPAddress x)
            {
                return resolvedDictionary.TryGetValue(x.ToString(), out var name) ? name : null;
            }
            string ResolveProcessName(FlowKey flowKey)
            {
                return resolvedDictionary.TryGetValue($"{flowKey.DestinationAddress}:{flowKey.DestinationPort}", out var processName) ? processName: null;
            }

            var osInfo = value.Payload.HostTags.Where(x => x.Name == "os_by_tcpip").FirstOrDefault()?.Value;

            var upflows = value.Payload.Flows.Where(x => x.SourceAddress.Equals(value.Payload.HostAddress)).ToList();
            
            var downflows = value.Payload.Flows.Where(x => x.DestinationAddress.Equals(value.Payload.HostAddress)).ToList();

            var initiatedConnections = GetInitiatedConnections(upflows, ResolveDomain).ToArray();

            var acceptedConnections = GetAcceptedConnections(downflows, ResolveDomain).ToArray();

            var webUrls = upflows.SelectFlows<HttpFlow, WebRequestInfo>(x => new WebRequestInfo(x.DestinationAddress, ResolveDomain(x.DestinationAddress), x.DestinationPort, ResolveProcessName(x.FlowKey), x.Method, x.Hostname + x.Url)).ToArray();
            
            var secured = upflows.SelectFlows<TlsFlow, TlsConnectionInfo>(x => new TlsConnectionInfo(x.DestinationAddress, ResolveDomain(x.DestinationAddress), x.DestinationPort, ResolveProcessName(x.FlowKey), x.ApplicationLayerProtocolNegotiation, x.ServerNameIndication, x.JA3Fingerprint, x.IssuerCommonName, x.SubjectCommonName, x.SubjectOrganisationName)).ToArray();
            
            var simpleContext = new IpSimpleHostContext(value.Payload.HostAddress, osInfo, initiatedConnections, acceptedConnections, domains, webUrls, secured);
            
            _subject.OnNext(new ObservableEvent<IpSimpleHostContext>(simpleContext, value.StartTime, value.EndTime));
        }

        private IEnumerable<IpConnectionInfo> GetInitiatedConnections(IEnumerable<IpFlow> flows, Func<IPAddress,string> resolveDomain)
        { 
            var con =  flows.Select(f =>
                        new IpConnectionInfo(f.DestinationAddress, resolveDomain(f.DestinationAddress), f.DestinationPort, f.ApplicationTag, 1, f.SentPackets, f.SentOctets, f.RecvPackets, f.RecvOctets));
            return con.GroupBy(key => (key.RemoteHostAddress, key.RemotePort),
                        (key, val) =>
                        {
                            var first = val.FirstOrDefault();
                            return new IpConnectionInfo(key.RemoteHostAddress, first?.RemoteHostName, key.RemotePort, first?.Service, val.Count(), val.Sum(x => x.PacketsSent), val.Sum(x => x.OctetsSent),
                                val.Sum(x => x.PacketsRecv), val.Sum(x => x.OctetsRecv));
                        }
                    );
        }
        private IEnumerable<IpConnectionInfo> GetAcceptedConnections(IEnumerable<IpFlow> flows, Func<IPAddress, string> resolveDomain)
        {
            var con = flows.Select(f =>
                        new IpConnectionInfo(f.SourceAddress, resolveDomain(f.SourceAddress), f.SourcePort, f.ApplicationTag, 1, f.SentPackets, f.SentOctets, f.RecvPackets, f.RecvOctets));
            return con.GroupBy(key => (key.RemoteHostAddress, key.RemotePort),
                        (key, val) =>
                        {
                            var first = val.FirstOrDefault();
                            return new IpConnectionInfo(key.RemoteHostAddress, first?.RemoteHostName, key.RemotePort, first?.Service, val.Count(), val.Sum(x => x.PacketsSent), val.Sum(x => x.OctetsSent),
                                val.Sum(x => x.PacketsRecv), val.Sum(x => x.OctetsRecv));
                        }
                    );
        }

        private Dictionary<string, string> GetDomainDictionary(ResolvedDomainInfo[] domains)
        {
            var dict = new Dictionary<string, string>();
            foreach(var d in domains)
            {
                var ip = d.ResponseData?.ToString() ?? String.Empty;
                dict[ip] = d.QueryString;
            }
            return dict;
        }

        public IDisposable Subscribe(IObserver<ObservableEvent<IpSimpleHostContext>> observer)
        {
            return _subject.Subscribe(observer);    
        }
    }


    public record IpSimpleHostContext(IPAddress HostAddress, string OperatingSystem, IpConnectionInfo[] InitiatedConnections, IpConnectionInfo[]AcceptedConnections, ResolvedDomainInfo[] ResolvedDomains, WebRequestInfo[] WebUrls, TlsConnectionInfo[] Secured);

    public record IpConnectionInfo(IPAddress RemoteHostAddress, string RemoteHostName, ushort RemotePort, string Service, int Flows, int PacketsSent, int OctetsSent, int PacketsRecv, int OctetsRecv);

    public record WebRequestInfo(IPAddress RemoteHostAddress, string RemoteHostName, ushort RemotePort, string ApplicationProcessName, string Method, string Url);

    public record ResolvedDomainInfo(IPAddress DnsServer, string QueryString, string ResponseData, DnsResponseCode ResponseCode);

    public record TlsConnectionInfo(IPAddress RemoteHostAddress, string RemoteHostName, ushort RemotePort, string ApplicationProcessName, string ApplicationProtocol, string ServerNameIndication, string JA3Fingerprint, string IssuerName, string SubjectName, string OrganisationName);
}
