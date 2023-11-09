using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers.TagObjects;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Polishers
{

    public static class IpHostContextPolisherCatalogEntry
    {
        public static IObservableTransformer<ObservableEvent<IpHostContextWithTags>, ObservableEvent<IpTargetHostContext>> GetContextPolisher(this ContextTransformCatalog catalog)
        {
            return new IpHostContextPolisher(catalog.Environment.Logger);
        }
    }
    /// <summary>
    /// Transforms a rich IP host context into a simplified IP host context.
    /// </summary>
    /// <remarks>
    /// The <see cref="IpHostContextPolisher"/> class is designed to take in an IP host context with abundant details (represented by <see cref="IpHostContextWithTags"/>) 
    /// and transform it into a more streamlined or simplified format, represented by <see cref="IpTargetHostContext"/>.
    /// 
    /// This transformation can be essential for scenarios where only specific details of the IP host are required, 
    /// thus reducing overhead and optimizing performance or readability.
    /// 
    /// The class implements the <see cref="IObservableTransformer{TInput,TOutput}"/> interface, indicating its ability to observe
    /// events of type <see cref="IpHostContextWithTags"/> and produce transformed events of type <see cref="IpTargetHostContext"/>.
    /// Additionally, by implementing the <see cref="IPipelineNode"/> interface, it suggests that this class plays a role in a processing pipeline, 
    /// as a node that performs specific transformations.
    /// </remarks>
    public class IpHostContextPolisher : IObservableTransformer<ObservableEvent<IpHostContextWithTags>, ObservableEvent<IpTargetHostContext>>
    {
        ILogger _logger;
        // We use subject as the simplest way to implement the transformer.
        // For the production version, consider more performant implementation.
        private Subject<ObservableEvent<IpTargetHostContext>> _subject;

        /// Represents a mechanism for retrieving an already computed result or 
        /// for signaling the completion of some asynchronous operation. 
        private TaskCompletionSource _tcs = new TaskCompletionSource();


        /// <summary>
        /// Gets the type of pipeline node represented by this instance, which is always Transformer.
        /// </summary>
        public PipelineNodeType NodeType => PipelineNodeType.Transformer;

        public Task Completed => _tcs.Task;

        /// <summary>
        /// Creates a new instance of the IpHostContextSimplifier class.
        /// </summary>
        public IpHostContextPolisher(ILogger logger = null)
        {
            _subject = new Subject<ObservableEvent<IpTargetHostContext>>();
            _logger = logger;
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            _subject.OnCompleted();
            _tcs.SetResult();
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
        public void OnNext(ObservableEvent<IpHostContextWithTags> value)
        {
            try
            {
                var domains = value.Payload.Flows.SelectFlows<DnsFlow>()
                    .Select(x => new ResolvedDomainInfo(x.DestinationAddress.ToString(), x.QuestionName, x.ResponseData, x.ResponseCode))
                    .ToArray();

                var domainResolver = new Resolver<string, ResolvedDomainInfo>(domains, d => d.ResponseData?.ToString() ?? String.Empty);

                var processResolver = new Resolver<string, TcpFlowTag>(value.Payload.Tags.Where(x => x.Type == nameof(TcpFlowTag)).Select<TagObject, TcpFlowTag>(x => x.GetDetailsAs<TcpFlowTag>()), flowTag => $"{flowTag.LocalAddress}:{flowTag.LocalPort}-{flowTag.RemoteAddress}:{flowTag.RemotePort}");

                var serviceResolver = new Resolver<string, (string, InternetServiceTag[])>(value.Payload.Tags.Where(x => x.Type == "NetifyIp").GroupBy(g => g.Key, (k, v) => (k, v.Select(x => new InternetServiceTag(x.Value, (float)x.Reliability)).ToArray())), t => t.Item1);

                string ResolveDomain(string address)
                {
                    return domainResolver.Resolve(address, x => x.QueryString);
                }
                string ResolveProcessName(FlowKey flowKey)
                {
                    return processResolver.Resolve($"{flowKey.SourceAddress}:{flowKey.SourcePort}-{flowKey.DestinationAddress}:{flowKey.DestinationPort}", x => x.ProcessName);
                }
                InternetServiceTag[] ResolveServices(string destinationAddress)
                {
                    return serviceResolver.Resolve(destinationAddress).Item2;
                }

                var connections = GetConnections(value.Payload.HostAddress, value.Payload.Flows, ResolveDomain, ResolveServices).ToArray();

                var webUrls = value.Payload.Flows.SelectFlows<HttpFlow, WebRequestInfo>(x => new WebRequestInfo(x.DestinationAddress.ToString(), ResolveDomain(x.DestinationAddress.ToString()), x.DestinationPort, ResolveProcessName(x.FlowKey), ResolveServices(x.DestinationAddress.ToString()), x.Method, x.Hostname + x.Url)).ToArray();

                var handshakes = value.Payload.Flows.SelectFlows<TlsFlow>().Where(x => !string.IsNullOrWhiteSpace(x.JA3Fingerprint)).Select(x => new TlsHandshakeInfo(x.DestinationAddress.ToString(), ResolveDomain(x.DestinationAddress.ToString()), x.DestinationPort, ResolveProcessName(x.FlowKey), ResolveServices(x.DestinationAddress.ToString()), x.ApplicationLayerProtocolNegotiation, x.ServerNameIndication, x.JA3Fingerprint, x.IssuerCommonName, x.SubjectCommonName, x.SubjectOrganisationName, x.CipherSuites, x.EllipticCurves)).ToArray();

                var simpleContext = new IpTargetHostContext(value.Payload.HostAddress, connections, domains, webUrls, handshakes);

                _subject.OnNext(new ObservableEvent<IpTargetHostContext>(simpleContext, value.StartTime, value.EndTime));
            }
            catch(Exception e)
            {
                _logger?.LogError(e, "Error in context polishing.", value);
                _subject.OnError(e);
            }
        }

        /// <summary>
        /// Returns an enumerable collection of initiated IP connections, based on the specified collection of IP flows and the provided function for resolving domain names.
        /// </summary>
        /// <param name="flows">The collection of IP flows to use as a source for the initiated connections.</param>
        /// <param name="resolveDomain">A function that takes an IP address as input and returns a domain name.</param>
        /// <returns>An enumerable collection of initiated IP connections.</returns>
        private IEnumerable<IpConnectionInfo> GetConnections(IPAddress hostAddress, IEnumerable<IpFlow> flows, Func<string, string> resolveDomain, Func<string, InternetServiceTag[]> resolveServices)
        {
            // Perform processing on the input flows
            var connections = flows.Select(f => f.SourceAddress.Equals(hostAddress) 
                ? new IpConnectionInfo(f.DestinationAddress.ToString(),null, f.DestinationPort, null, null, 1, f.SentPackets, f.SentOctets, f.RecvPackets, f.RecvOctets)
                : new IpConnectionInfo(f.SourceAddress.ToString(), null, f.SourcePort, null, null, 1, f.RecvPackets, f.RecvOctets, f.SentPackets, f.SentOctets)
            );
            // Return the resulting collection of initiated connections
            return connections.GroupBy(key => (key.RemoteHostAddress, key.RemotePort),
                        (key, val) =>
                        {
                            var address = key.RemoteHostAddress;
                                return new IpConnectionInfo(address, resolveDomain(address), key.RemotePort, null, resolveServices(address),
                                    val.Count(), val.Sum(x => x.PacketsSent), val.Sum(x => x.OctetsSent),
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
