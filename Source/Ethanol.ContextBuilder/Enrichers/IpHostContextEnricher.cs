using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using YamlDotNet.Core.Tokens;
using static System.Reflection.Metadata.BlobBuilder;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class RawHostContext : IpHostContext<TagObject[]> { }
    /// <summary>
    /// Implements a transformer that enrich <see cref="IpHostContext"/> and produces <see cref="RawHostContext"/>.
    /// </summary>
    public class IpHostContextEnricher : IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<RawHostContext>>, IPipelineNode
    { 
        private readonly Subject<ObservableEvent<RawHostContext>> _subject;
        private readonly ITagDataProvider<TagObject> _tagQueryable;

        /// <summary>
        /// Creates an instance of the context enricher where the additional data sources are <paramref name="hostTagQueryable"/> and <paramref name="flowTagQueryable"/>.
        /// </summary>
        /// <param name="hostTagQueryable">The queryable for the enviroment.</param>
        /// <param name="flowTagQueryable">The queryable for the state.</param>
        public IpHostContextEnricher(ITagDataProvider<TagObject> tagQueryable)
        {
            _subject = new Subject<ObservableEvent<RawHostContext>>();
            _tagQueryable = tagQueryable;
        }

        public PipelineNodeType NodeType => PipelineNodeType.Transformer;

        /// <inheritdoc/>
        public void OnCompleted()
        {
            _subject.OnCompleted();
        }
        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }
        /// <inheritdoc/>
        public void OnNext(ObservableEvent<IpHostContext> value)
        {
            var tags = GetTags(value);
            _subject.OnNext(new ObservableEvent<RawHostContext>(new RawHostContext { HostAddress = value.Payload.HostAddress, Flows = value.Payload.Flows, Tags = tags.ToArray() },  value.StartTime, value.EndTime));
        }

        private IEnumerable<TagObject> GetTags(ObservableEvent<IpHostContext> value)
        {
            var host = value.Payload.HostAddress;
            var start = value.StartTime;
            var end = value.EndTime;
            var flows = value.Payload.Flows;
            var remoteHosts = flows.Select(x => x.GetRemoteAddress(host)).Distinct();
            if (_tagQueryable != null)
            {
                return  GetLocalTags(host, start, end).Concat(GetRemoteTags(remoteHosts, start, end)).Concat(GetFlowTags(flows, start, end));
            }
            return Array.Empty<TagObject>();
        }

        private IEnumerable<TagObject> GetLocalTags(IPAddress host, DateTime start, DateTime end)
        {
            return _tagQueryable?.Get(host.ToString(), start, end) ?? Enumerable.Empty<TagObject>();
        }
        /// <summary>
        /// This method takes an array of IpFlow objects and two DateTime values, and returns a dictionary
        /// where the key is a string representation of a remote address (from the IpFlow array), and the
        /// value is an array of NetifyApplication objects that match the given remote address and time range.
        /// If there are no matching NetifyApplication objects, an empty array is returned.
        /// </summary>
        /// <param name="flows">The array of IpFlow objects.</param>
        /// <param name="start">The start time of the time range.</param>
        /// <param name="end">The end time of the time range.</param>
        /// <returns>A dictionary where the key is a string representation of a remote address (from the IpFlow array), and the
        /// value is an array of NetifyApplication objects that match the given remote address and time range.
        /// </returns>
        private IEnumerable<TagObject> GetRemoteTags(IEnumerable<IPAddress> remoteHosts, DateTime start, DateTime end)
        {                
            return remoteHosts.SelectMany(addr => _tagQueryable?.Get(addr.ToString(), start, end) ?? Enumerable.Empty<TagObject>());
        }
        private IEnumerable<TagObject> GetFlowTags(IEnumerable<IpFlow> flows, DateTime start, DateTime end)
        {
            return flows.SelectMany(flow => _tagQueryable?.Get(flow.FlowKey.ToString(), start, end) ?? Enumerable.Empty<TagObject>());
        }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<ObservableEvent<RawHostContext>> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
