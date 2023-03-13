using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
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
        private readonly IHostDataProvider<HostTag> _hostTagQueryable;
        private readonly IHostDataProvider<FlowTag> _flowTagQueryable;

        /// <summary>
        /// Creates an instance of the context enricher where the additional data sources are <paramref name="hostTagQueryable"/> and <paramref name="flowTagQueryable"/>.
        /// </summary>
        /// <param name="hostTagQueryable">The queryable for the enviroment.</param>
        /// <param name="flowTagQueryable">The queryable for the state.</param>
        public IpHostContextEnricher(IHostDataProvider<HostTag> hostTagQueryable, IHostDataProvider<FlowTag> flowTagQueryable)
        {
            _subject = new Subject<ObservableEvent<IpRichHostContext>>();
            _hostTagQueryable = hostTagQueryable;
            _flowTagQueryable = flowTagQueryable;
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
            var envTags = (_hostTagQueryable?.Get(host.ToString(), start, end) ?? Enumerable.Empty<HostTag>()).ToArray();
                        
            // collects tags related to host's flows...
            var flowTags = (_flowTagQueryable?.Get(host.ToString(), start, end) ?? Enumerable.Empty<FlowTag>()).ToArray();
            
            _subject.OnNext(new ObservableEvent<IpRichHostContext>(new IpRichHostContext(value.Payload.HostAddress, value.Payload.Flows, envTags, flowTags), value.StartTime, value.EndTime));
        }
        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<ObservableEvent<IpRichHostContext>> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
