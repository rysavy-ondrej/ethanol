using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Observable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;

namespace Ethanol.ContextBuilder.Simplifier
{
    public class IpHostContextSimplifierPlugin : IObservableTransformer
    {
        public string TransformerName => nameof(IpHostContextSimplifier);

        public Type SourceType => typeof(IpRichHostContext);

        public Type TargetType => typeof(IpSimpleHostContext);

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(object value)
        {
            throw new NotImplementedException();
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            throw new NotImplementedException();
        }
    }

    public class IpHostContextSimplifier : IObservableTransformer<ObservableEvent<IpRichHostContext>, ObservableEvent<IpSimpleHostContext>>
    {
        // We use subject as the simplest way to implement the transformer.
        // For the production version, consider more performant implementation.
        private Subject<ObservableEvent<IpSimpleHostContext>> _subject;

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
                .Where(x=>x.QueryResponseFlag == DnsQueryResponseFlag.Response)
                .Select(x=>new ResolvedDomainInfo(x.SourceAddress, x.QuestionName, x.ResponseData, x.ResponseCode))
                .ToArray();
            
            var resolvedDictionary = GetDomainDictionary(domains);
            var resolveDomain = (IPAddress x) => resolvedDictionary.TryGetValue(x.ToString(), out var name) ? name : null;

            var osInfo = value.Payload.Metadata.Where(x => x.Name == "os_by_tcpip").FirstOrDefault()?.Value;
            var upflows = value.Payload.Flows.Where(x => x.SourceAddress.Equals(value.Payload.HostAddress)).ToList();
            var downflows = value.Payload.Flows.Where(x => x.DestinationAddress.Equals(value.Payload.HostAddress)).ToList();

            var connections = PairFlows(upflows, downflows, resolveDomain)
                .GroupBy(key => (key.RemoteHostAddress, key.RemotePort),
                        (key,val) =>
                        {
                            var first = val.FirstOrDefault();
                            return new IpConnectionInfo(key.RemoteHostAddress, first?.RemoteHostName, key.RemotePort, first?.Service, val.Count(), val.Sum(x => x.PacketsSent), val.Sum(x => x.OctetsSent),
                                val.Sum(x => x.PacketsRecv), val.Sum(x => x.OctetsRecv));
                        }
                    )
                .ToArray();   
            


            var webUrls = upflows.SelectFlows<HttpFlow, WebRequestInfo>(x => new WebRequestInfo(x.DestinationAddress, resolveDomain(x.DestinationAddress), x.DestinationPort, x.Method, x.Hostname + x.Url)).ToArray();
            var secured = upflows.SelectFlows<TlsFlow, TlsConnectionInfo>(x => new TlsConnectionInfo(x.DestinationAddress, resolveDomain(x.DestinationAddress), x.DestinationPort, x.ApplicationLayerProtocolNegotiation, x.ServerNameIndication, x.JA3Fingerprint, x.IssuerCommonName, x.SubjectCommonName, x.SubjectOrganisationName)).ToArray();
            var simpleContext = new IpSimpleHostContext(value.Payload.HostAddress, osInfo, connections, domains, webUrls, secured);
            _subject.OnNext(new ObservableEvent<IpSimpleHostContext>(simpleContext, value.StartTime, value.EndTime));
        }

        private IEnumerable<IpConnectionInfo> PairFlows(IEnumerable<IpFlow> upflows, IEnumerable<IpFlow> downFlows, Func<IPAddress,string> resolveDomain)
        { 
            var biflows = upflows.Join(downFlows, 
                    f => (f.SourceAddress, f.SourcePort), 
                    f => (f.DestinationAddress, f.DestinationPort), 
                    (f1, f2) =>
                        new IpConnectionInfo(f2.SourceAddress, resolveDomain(f2.SourceAddress), f2.SourcePort, f2.ApplicationTag, 1, f1.PacketDeltaCount, f1.OctetDeltaCount, f2.PacketDeltaCount, f2.OctetDeltaCount));
            return biflows;
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


    public record IpSimpleHostContext(IPAddress HostAddress, string OperatingSystem, IpConnectionInfo[] ConnectsTo, ResolvedDomainInfo[] ResolvedDomains, WebRequestInfo[] WebUrls, TlsConnectionInfo[] Secured);

    public record IpConnectionInfo(IPAddress RemoteHostAddress, string RemoteHostName, ushort RemotePort, string Service, int Flows, int PacketsSent, int OctetsSent, int PacketsRecv, int OctetsRecv);

    public record WebRequestInfo(IPAddress RemoteHostAddress, string RemoteHostName, ushort RemotePort, string Method, string Url);

    public record ResolvedDomainInfo(IPAddress RemoteHost, string QueryString, string ResponseData, DnsResponseCode ResponseCode);

    public record TlsConnectionInfo
    {
        public TlsConnectionInfo(IPAddress remoteHostAddress, string remoteHostName, ushort remotePort, string applicationProtocol, string serverNameIndication, string jA3Fingerprint, string issuerName, string subjectName, string organisationName)
        {
            RemoteHostAddress = remoteHostAddress;
            RemoteHostName = remoteHostName;
            RemotePort = remotePort;
            ApplicationProtocol = applicationProtocol;
            ServerNameIndication = serverNameIndication;
            JA3Fingerprint = jA3Fingerprint;
            IssuerName = issuerName;
            SubjectName = subjectName;
            OrganisationName = organisationName;
        }

        public IPAddress RemoteHostAddress { get; init; }
        public string RemoteHostName { get; init; }
        public ushort RemotePort { get; init; }
        public bool IsEncrypted { get; init; }
        public string ApplicationProtocol { get; init; }
        public string ServerNameIndication { get; init; }
        public string JA3Fingerprint { get; init; }
        public string IssuerName { get; init; }
        public string SubjectName { get; init; }
        public string OrganisationName { get; init; }
    }
}
