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
    /// <summary>
    /// Transforms a rich IP host context into a simplified IP host context.
    /// </summary>
    public class IpHostContextSimplifier : IObservableTransformer<ObservableEvent<IpRichHostContext>, ObservableEvent<IpSimpleHostContext>>, IPipelineNode
    {
        // We use subject as the simplest way to implement the transformer.
        // For the production version, consider more performant implementation.
        private Subject<ObservableEvent<IpSimpleHostContext>> _subject;

        /// <summary>
        /// Gets the type of pipeline node represented by this instance, which is always Transformer.
        /// </summary>
        public PipelineNodeType NodeType => PipelineNodeType.Transformer;

        /// <summary>
        /// Creates a new instance of the IpHostContextSimplifier class.
        /// </summary>
        public IpHostContextSimplifier()
        {
            _subject = new Subject<ObservableEvent<IpSimpleHostContext>>();
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">The exception that describes the error condition.</param>
        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        /// <summary>
        /// Performs transformation on the new input in the form of an observable event containing a rich IP host context.
        /// </summary>
        /// <param name="value">The input observable event containing a rich IP host context.</param>
        public void OnNext(ObservableEvent<IpRichHostContext> value)
        {
            var domains = value.Payload.Flows.SelectFlows<DnsFlow>()
                .Select(x=>new ResolvedDomainInfo(x.SourceAddress, x.QuestionName, x.ResponseData, x.ResponseCode))
                .ToArray();
            
            var resolvedDictionary = GetDomainDictionary(domains);

            var flowTagDictionary = GetFlowTagDictionary(value.Payload.FlowTags); 

            string ResolveDomain(IPAddress x)
            {
                return resolvedDictionary.TryGetValue(x.ToString(), out var name) ? name : null;
            }
            string ResolveProcessName(FlowKey flowKey)
            {
                return flowTagDictionary.TryGetValue($"{flowKey.SourceAddress}:{flowKey.SourcePort}-{flowKey.DestinationAddress}:{flowKey.DestinationPort}", out var flowTag) ? flowTag.ProcessName : null;
            }

            var osInfo = value.Payload.HostTags.Where(x => x.Name == "os_by_tcpip").FirstOrDefault()?.Value;

            var upflows = value.Payload.Flows.Where(x => x.SourceAddress.Equals(value.Payload.HostAddress)).ToList();
            
            var downflows = value.Payload.Flows.Where(x => x.DestinationAddress.Equals(value.Payload.HostAddress)).ToList();

            var initiatedConnections = GetInitiatedConnections(upflows, ResolveDomain).ToArray();

            var acceptedConnections = GetAcceptedConnections(downflows, ResolveDomain).ToArray();

            var webUrls = upflows.SelectFlows<HttpFlow, WebRequestInfo>(x => new WebRequestInfo(x.DestinationAddress, ResolveDomain(x.DestinationAddress), x.DestinationPort, ResolveProcessName(x.FlowKey), x.Method, x.Hostname + x.Url)).ToArray();
            
            var handshakes = upflows.SelectFlows<TlsFlow>().Where(x => !String.IsNullOrWhiteSpace(x.JA3Fingerprint)).Select(x => new TlsHandshakeInfo(x.DestinationAddress, ResolveDomain(x.DestinationAddress), x.DestinationPort, ResolveProcessName(x.FlowKey), x.ApplicationLayerProtocolNegotiation, x.ServerNameIndication, x.JA3Fingerprint, x.IssuerCommonName, x.SubjectCommonName, x.SubjectOrganisationName)).ToArray();
            
            var simpleContext = new IpSimpleHostContext(value.Payload.HostAddress, osInfo, initiatedConnections, acceptedConnections, domains, webUrls, handshakes);
            
            _subject.OnNext(new ObservableEvent<IpSimpleHostContext>(simpleContext, value.StartTime, value.EndTime));
        }

        /// <summary>
        /// Returns a dictionary that maps each unique combination of LocalAddress, LocalPort, RemoteAddress, and RemotePort to its corresponding FlowTag object.
        /// <para/>
        /// The method iterates over each FlowTag in the input array and generates a key by concatenating the LocalAddress, LocalPort, RemoteAddress, and RemotePort fields.
        /// The key is then used to add the FlowTag object to a dictionary.If a key already exists in the dictionary, the corresponding FlowTag object is overwritten.
        /// </summary>
        /// <param name="flowTags">An array of FlowTag objects.</param>
        /// <returns>A Dictionary  that maps each unique combination of LocalAddress, LocalPort, RemoteAddress, and RemotePort to its corresponding FlowTag object.</returns>
        private Dictionary<string,FlowTag> GetFlowTagDictionary(FlowTag[] flowTags)
        {
            var dicitonary = new Dictionary<string, FlowTag>(); 

            foreach (var flowTag in flowTags)
            {
                var key = $"{flowTag.LocalAddress}:{flowTag.LocalPort}-{flowTag.RemoteAddress}:{flowTag.RemotePort}";
                dicitonary[key] = flowTag;
            }
            return dicitonary;
        }

        /// <summary>
        /// Returns an enumerable collection of initiated IP connections, based on the specified collection of IP flows and the provided function for resolving domain names.
        /// </summary>
        /// <param name="flows">The collection of IP flows to use as a source for the initiated connections.</param>
        /// <param name="resolveDomain">A function that takes an IP address as input and returns a domain name.</param>
        /// <returns>An enumerable collection of initiated IP connections.</returns>
        private IEnumerable<IpConnectionInfo> GetInitiatedConnections(IEnumerable<IpFlow> flows, Func<IPAddress,string> resolveDomain)
        {
            // Perform processing on the input flows
            var con =  flows.Select(f =>
                        new IpConnectionInfo(f.DestinationAddress, resolveDomain(f.DestinationAddress), f.DestinationPort, f.ApplicationTag, 1, f.SentPackets, f.SentOctets, f.RecvPackets, f.RecvOctets));
            // Return the resulting collection of initiated connections
            return con.GroupBy(key => (key.RemoteHostAddress, key.RemotePort),
                        (key, val) =>
                        {
                            var first = val.FirstOrDefault();
                            return new IpConnectionInfo(key.RemoteHostAddress, first?.RemoteHostName, key.RemotePort, first?.Service, val.Count(), val.Sum(x => x.PacketsSent), val.Sum(x => x.OctetsSent),
                                val.Sum(x => x.PacketsRecv), val.Sum(x => x.OctetsRecv));
                        }
                    );
        }
        /// <summary>
        /// Returns an enumerable collection of accepted IP connections, based on the specified collection of IP flows and the provided function for resolving domain names.
        /// </summary>
        /// <param name="flows">The collection of IP flows to use as a source for the accepted connections.</param>
        /// <param name="resolveDomain">A function that takes an IP address as input and returns a domain name.</param>
        /// <returns>An enumerable collection of accepted IP connections.</returns>
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

        /// <summary>
        /// Creates a dictionary of resolved domain information, mapping each IP address to its corresponding query string.
        /// </summary>
        /// <param name="domains">An array of resolved domain information.</param>
        /// <returns>A Dictionary that maps each IP address to its corresponding query string.</returns>
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
        /// <summary>
        /// Subscribes an observer to the output observable sequence of simplified IP host contexts.
        /// </summary>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns>An IDisposable object that can be used to unsubscribe the observer.</returns>
        public IDisposable Subscribe(IObserver<ObservableEvent<IpSimpleHostContext>> observer)
        {
            return _subject.Subscribe(observer);    
        }
    }
  }
