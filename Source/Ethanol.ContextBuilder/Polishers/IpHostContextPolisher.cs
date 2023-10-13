using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;


namespace Ethanol.ContextBuilder.Polishers
{
    /// <summary>
    /// Transforms a rich IP host context into a simplified IP host context.
    /// </summary>
    /// <remarks>
    /// The <see cref="IpHostContextPolisher"/> class is designed to take in an IP host context with abundant details (represented by <see cref="RawHostContext"/>) 
    /// and transform it into a more streamlined or simplified format, represented by <see cref="IpTargetHostContext"/>.
    /// 
    /// This transformation can be essential for scenarios where only specific details of the IP host are required, 
    /// thus reducing overhead and optimizing performance or readability.
    /// 
    /// The class implements the <see cref="IObservableTransformer{TInput,TOutput}"/> interface, indicating its ability to observe
    /// events of type <see cref="RawHostContext"/> and produce transformed events of type <see cref="IpTargetHostContext"/>.
    /// Additionally, by implementing the <see cref="IPipelineNode"/> interface, it suggests that this class plays a role in a processing pipeline, 
    /// as a node that performs specific transformations.
    /// </remarks>
    public class IpHostContextPolisher : IObservableTransformer<ObservableEvent<RawHostContext>, ObservableEvent<IpTargetHostContext>>, IPipelineNode
    {
        // We use subject as the simplest way to implement the transformer.
        // For the production version, consider more performant implementation.
        private Subject<ObservableEvent<IpTargetHostContext>> _subject;

        /// <summary>
        /// Gets the type of pipeline node represented by this instance, which is always Transformer.
        /// </summary>
        public PipelineNodeType NodeType => PipelineNodeType.Transformer;

        /// <summary>
        /// Creates a new instance of the IpHostContextSimplifier class.
        /// </summary>
        public IpHostContextPolisher()
        {
            _subject = new Subject<ObservableEvent<IpTargetHostContext>>();
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
        public void OnNext(ObservableEvent<RawHostContext> value)
        {
            var domains = value.Payload.Flows.SelectFlows<DnsFlow>()
                .Select(x=>new ResolvedDomainInfo(x.DestinationAddress.ToString(), x.QuestionName, x.ResponseData, x.ResponseCode))
                .ToArray();

            var domainResolver = new Resolver<string,ResolvedDomainInfo>(domains, d => d.ResponseData?.ToString() ?? String.Empty);

            var processResolver = new Resolver<string, FlowTag>(value.Payload.Tags.Where(x => x.Type == nameof(FlowTag)).Select<TagObject,FlowTag>(x=>x.GetDetailsAs<FlowTag>()), flowTag => $"{flowTag.LocalAddress}:{flowTag.LocalPort}-{flowTag.RemoteAddress}:{flowTag.RemotePort}");

            string ResolveDomain(IPAddress x)
            {
                return domainResolver.Resolve(x.ToString(), x => x.QueryString);
            }
            string ResolveProcessName(FlowKey flowKey)
            {
                return processResolver.Resolve($"{flowKey.SourceAddress}:{flowKey.SourcePort}-{flowKey.DestinationAddress}:{flowKey.DestinationPort}", x => x.ProcessName);
            }

            var tags = value.Payload.Tags;

            var upflows = value.Payload.Flows.Where(x => x.SourceAddress.Equals(value.Payload.HostAddress)).ToList();
            
            var downflows = value.Payload.Flows.Where(x => x.DestinationAddress.Equals(value.Payload.HostAddress)).ToList();

            var initiatedConnections = GetInitiatedConnections(upflows, ResolveDomain, ResolveProcessName).ToArray();

            var acceptedConnections = GetAcceptedConnections(downflows, ResolveDomain, ResolveProcessName).ToArray();

            var webUrls = upflows.SelectFlows<HttpFlow, WebRequestInfo>(x => new WebRequestInfo(x.DestinationAddress.ToString(), ResolveDomain(x.DestinationAddress), x.DestinationPort, ResolveProcessName(x.FlowKey), x.Method, x.Hostname + x.Url)).ToArray();
            
            var handshakes = upflows.SelectFlows<TlsFlow>().Where(x => !String.IsNullOrWhiteSpace(x.JA3Fingerprint)).Select(x => new TlsHandshakeInfo(x.DestinationAddress.ToString(), ResolveDomain(x.DestinationAddress), x.DestinationPort, ResolveProcessName(x.FlowKey), x.ApplicationLayerProtocolNegotiation, x.ServerNameIndication, x.JA3Fingerprint, x.IssuerCommonName, x.SubjectCommonName, x.SubjectOrganisationName, x.CipherSuites, x.EllipticCurves)).ToArray();
            
            var simpleContext = new IpTargetHostContext(value.Payload.HostAddress, tags, initiatedConnections, acceptedConnections, domains, webUrls, handshakes);
            
            _subject.OnNext(new ObservableEvent<IpTargetHostContext>(simpleContext, value.StartTime, value.EndTime));
        }
 
        /// <summary>
        /// Returns an enumerable collection of initiated IP connections, based on the specified collection of IP flows and the provided function for resolving domain names.
        /// </summary>
        /// <param name="flows">The collection of IP flows to use as a source for the initiated connections.</param>
        /// <param name="resolveDomain">A function that takes an IP address as input and returns a domain name.</param>
        /// <returns>An enumerable collection of initiated IP connections.</returns>
        private IEnumerable<IpConnectionInfo> GetInitiatedConnections(IEnumerable<IpFlow> flows, Func<IPAddress,string> resolveDomain, Func<FlowKey, string> resolveProcessName)
        {
            // Perform processing on the input flows
            var con =  flows.Select(f =>
                        new IpConnectionInfo(f.DestinationAddress.ToString(), resolveDomain(f.DestinationAddress), f.DestinationPort, resolveProcessName(f.FlowKey), 1, f.SentPackets, f.SentOctets, f.RecvPackets, f.RecvOctets));
            // Return the resulting collection of initiated connections
            return con.GroupBy(key => (key.RemoteHostAddress, key.RemotePort),
                        (key, val) =>
                        {
                            var first = val.FirstOrDefault();
                            return new IpConnectionInfo(key.RemoteHostAddress, first?.RemoteHostName, key.RemotePort, first?.ApplicationProcessName, val.Count(), val.Sum(x => x.PacketsSent), val.Sum(x => x.OctetsSent),
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
        private IEnumerable<IpConnectionInfo> GetAcceptedConnections(IEnumerable<IpFlow> flows, Func<IPAddress, string> resolveDomain, Func<FlowKey, string> resolveProcessName)
        {
            var con = flows.Select(f =>
                        new IpConnectionInfo(f.SourceAddress.ToString(), resolveDomain(f.SourceAddress), f.SourcePort, resolveProcessName(f.FlowKey), 1, f.SentPackets, f.SentOctets, f.RecvPackets, f.RecvOctets));
            return con.GroupBy(key => (key.RemoteHostAddress, key.RemotePort),
                        (key, val) =>
                        {
                            var first = val.FirstOrDefault();
                            return new IpConnectionInfo(key.RemoteHostAddress, first?.RemoteHostName, key.RemotePort, first?.ApplicationProcessName, val.Count(), val.Sum(x => x.PacketsSent), val.Sum(x => x.OctetsSent),
                                val.Sum(x => x.PacketsRecv), val.Sum(x => x.OctetsRecv));
                        }
                    );
        }

        /// <summary>
        /// Subscribes an observer to the output observable sequence of simplified IP host contexts.
        /// </summary>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns>An IDisposable object that can be used to unsubscribe the observer.</returns>
        public IDisposable Subscribe(IObserver<ObservableEvent<IpTargetHostContext>> observer)
        {
            return _subject.Subscribe(observer);    
        }

        /// <summary>
        /// Provides a simple way to resolve a value by its key from a dictionary-like collection of values.
        /// </summary>
        /// <typeparam name="TKey">The type of the key used to index the values.</typeparam>
        /// <typeparam name="TValue">The type of the values to be stored and indexed.</typeparam>
        class Resolver<TKey, TValue>
        {
            Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
            /// <summary>
            /// Initializes a new instance of the <see cref="Resolver{TKey, TValue}"/> class with the specified collection of values and key selector function.
            /// </summary>
            /// <param name="values">An enumerable collection of <typeparamref name="TValue"/> objects to store in the resolver.</param>
            /// <param name="getKey">A function that takes a <typeparamref name="TValue"/> object and returns its corresponding key of type <typeparamref name="TKey"/>.</param>
            public Resolver(IEnumerable<TValue> values, Func<TValue, TKey> getKey)
            {
                foreach (var value in values)
                {
                    _dictionary[getKey(value)] = value;
                }
            }
            /// <summary>
            /// Resolves a value of type <typeparamref name="TValue"/> by the specified key.
            /// </summary>
            /// <param name="key">The key of type <typeparamref name="TKey"/> used to resolve the value.</param>
            /// <returns>The value of type <typeparamref name="TValue"/> that corresponds to the specified key, or the default value of <typeparamref name="TValue"/> if the key is not found.</returns>
            public TValue Resolve(TKey key)
            {
                return _dictionary.TryGetValue(key, out var value) ? value : default;
            }
            /// <summary>
            /// Resolves a value of type <typeparamref name="TResult"/> by the specified key, using the specified selector function to transform the resolved value.
            /// </summary>
            /// <typeparam name="TResult">The type of the result to be returned.</typeparam>
            /// <param name="key">The key of type <typeparamref name="TKey"/> used to resolve the value.</param>
            /// <param name="select">A <see cref="Func{T, TResult}"/> that transforms the resolved value of type <typeparamref name="TValue"/> into a result of type <typeparamref name="TResult"/>.</param>
            /// <returns>The transformed value of type <typeparamref name="TResult"/> that corresponds to the specified key, or the default value of <typeparamref name="TResult"/> if the key is not found.</returns>
            public TResult Resolve<TResult>(TKey key, Func<TValue, TResult> select)
            {
                return _dictionary.TryGetValue(key, out var value) ? select(value) : default;
            }
        }
    }
  }
