using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class IpRichHostContext : IpHostContext<ContextTag[]> { }
    /// <summary>
    /// Implements a transformer that enrich <see cref="IpHostContext"/> and produces <see cref="IpRichHostContext"/>.
    /// </summary>
    public class IpHostContextEnricher : IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpRichHostContext>>, IPipelineNode
    { 
        private readonly Subject<ObservableEvent<IpRichHostContext>> _subject;
        private readonly IHostDataProvider<HostTag> _hostTagQueryable;
        private readonly IHostDataProvider<FlowTag> _flowTagQueryable;
        private readonly IHostDataProvider<NetifyTag> _netifyTagQueryable;

        /// <summary>
        /// Creates an instance of the context enricher where the additional data sources are <paramref name="hostTagQueryable"/> and <paramref name="flowTagQueryable"/>.
        /// </summary>
        /// <param name="hostTagQueryable">The queryable for the enviroment.</param>
        /// <param name="flowTagQueryable">The queryable for the state.</param>
        public IpHostContextEnricher(IHostDataProvider<HostTag> hostTagQueryable, IHostDataProvider<FlowTag> flowTagQueryable, IHostDataProvider<NetifyTag> netifyTagQueryable)
        {
            _subject = new Subject<ObservableEvent<IpRichHostContext>>();
            _hostTagQueryable = hostTagQueryable;
            _flowTagQueryable = flowTagQueryable;
            _netifyTagQueryable = netifyTagQueryable;
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
            var host = value.Payload.HostAddress;
            var start = value.StartTime;
            var end = value.EndTime;

            // collects tags related to the host...
            var envTags = (_hostTagQueryable?.Get(host.ToString(), start, end) ?? Enumerable.Empty<HostTag>()).Select(x=> new ContextTag<string, HostTag> { Name = nameof(HostTag), Key = x.Name, Value = x, Reliability = x.Reliability}).Cast<ContextTag>();
                        
            // collects tags related to host's flows...
            var flowTags = (_flowTagQueryable?.Get(host.ToString(), start, end) ?? Enumerable.Empty<FlowTag>()).Select(x=> new ContextTag<FlowKey, FlowTag> { Name = nameof(FlowTag), Key = x.GetFlowKey(), Value =x, Reliability = 1.0}).Cast<ContextTag>();
            
            // collects web applications related to host's flows...
            var webApps = GetWebAppsForFlows(value.Payload.Flows,start, end).Select(x=> new ContextTag<IPAddress, NetifyTag[]> { Name = nameof(NetifyTag), Key = x.Key, Value = x.Value, Reliability = 1.0 }).Cast<ContextTag>();

            var tags = envTags.Concat(flowTags).Concat(webApps).ToArray();

            _subject.OnNext(new ObservableEvent<IpRichHostContext>(new IpRichHostContext { HostAddress = value.Payload.HostAddress, Flows = value.Payload.Flows, Tags = tags },  value.StartTime, value.EndTime));
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
        private IDictionary<IPAddress, NetifyTag[]> GetWebAppsForFlows(IpFlow[] flows, DateTime start, DateTime end)
        {
            var dictionary = new Dictionary<IPAddress, NetifyTag[]>();
            if (_netifyTagQueryable == null) return dictionary;           
            var remoteAddresses = flows.Select(x=>x.DestinationAddress).Distinct();
            foreach(var addr in remoteAddresses)
            {
                var apps = _netifyTagQueryable.Get(addr.ToString(), start, end) ?? Enumerable.Empty<NetifyTag>();
                dictionary.Add(addr, apps.ToArray());
            }
            return dictionary;
        }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<ObservableEvent<IpRichHostContext>> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
