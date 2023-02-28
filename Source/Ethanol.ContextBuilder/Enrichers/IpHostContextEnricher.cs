using Elastic.Clients.Elasticsearch.Fluent;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using System;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a record for enriched host contexts.
    /// </summary>
    /// <param name="HostAddress">The host address, which stands for the key of the record.</param>
    /// <param name="Flows">The array of associated flows.</param>
    /// <param name="Metadata">The array of metadata related to the host.</param>
    public record IpRichHostContext(IPAddress HostAddress, IpFlow[] Flows, HostTag[] Metadata);

    /// <summary>
    /// Enrich the computed context with additional known information.
    /// </summary>
    public class IpHostContextEnricher : IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpRichHostContext>>
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
            var envTags = _environmentQuerable.Get(host.ToString(), start, end);
            // get tags from the state:
            var stateTags = _stateQueryable.Get(host.ToString(), start, end);
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
