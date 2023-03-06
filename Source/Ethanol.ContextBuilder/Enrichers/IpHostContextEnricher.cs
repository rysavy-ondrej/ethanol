using Elastic.Clients.Elasticsearch.Fluent;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using System;
using System.Linq;
using System.Reactive.Subjects;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Implements a transformer that enrich <see cref="IpHostContext"/> and produces <see cref="IpRichHostContext"/>.
    /// </summary>
    public class IpHostContextEnricher : IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpRichHostContext>>, IPipelineNode
    { 
        private readonly Subject<ObservableEvent<IpRichHostContext>> _subject;
        private readonly IHostDataProvider<HostTag> _environmentQuerable;
        private readonly IHostDataProvider<HostTag> _stateQueryable;

        /// <summary>
        /// Creates an instance of the context enricher where the additional data sources are <paramref name="environmentQuerable"/> and <paramref name="stateQueryable"/>.
        /// </summary>
        /// <param name="environmentQuerable">The queryable for the enviroment.</param>
        /// <param name="stateQueryable">The queryable for the state.</param>
        public IpHostContextEnricher(IHostDataProvider<HostTag> environmentQuerable, IHostDataProvider<HostTag> stateQueryable)
        {
            _subject = new Subject<ObservableEvent<IpRichHostContext>>();
            _environmentQuerable = environmentQuerable;
            _stateQueryable = stateQueryable;
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

            // get environment tags:
            var envTags = _environmentQuerable?.Get(host.ToString(), start, end) ?? Enumerable.Empty<HostTag>();
            // get tags from the state:
            var stateTags = _stateQueryable?.Get(host.ToString(), start, end) ?? Enumerable.Empty<HostTag>();
            // combine the tags in a single array:
            var tagArray = envTags.Concat(stateTags).ToArray();

            _subject.OnNext(new ObservableEvent<IpRichHostContext>(new IpRichHostContext(value.Payload.HostAddress, value.Payload.Flows, tagArray), value.StartTime, value.EndTime));
        }
        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<ObservableEvent<IpRichHostContext>> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
