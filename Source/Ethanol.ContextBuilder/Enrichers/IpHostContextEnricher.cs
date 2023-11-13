using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Enriches the context of IP hosts by applying additional data from provided data sources.
    /// </summary>
    public class IpHostContextEnricher : IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>>
    {
        ILogger _logger;
        /// Represents a subject that can both observe items of type <see cref="ObservableEvent<RawHostContext>"/> 
        /// as well as produce them. This is often used to represent both the source and terminator of an observable sequence.
        private readonly Subject<ObservableEvent<IpHostContextWithTags>> _subject;

        /// Provides a mechanism for querying tag data. It can be used to retrieve or fetch tags associated with certain data or context.
        private readonly ITagDataProvider<TagObject, IpHostContext> _tagProvider;

        /// Represents a mechanism for signaling the completion of some asynchronous operation. 
        /// This provides a way to manually control the lifetime of a Task, signaling its completion.
        private TaskCompletionSource _tcs = new TaskCompletionSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="IpHostContextEnricher"/> class.
        /// </summary>
        /// <param name="tagQueryable">Provides querying capabilities for tags.</param>
        public IpHostContextEnricher(ITagDataProvider<TagObject, IpHostContext> tagProvider, ILogger logger = null)
        {
            _subject = new Subject<ObservableEvent<IpHostContextWithTags>>();
            _tagProvider = tagProvider;
            _logger = logger;
        }

        public Task Completed => _tcs.Task;

        /// <summary>
        /// Signals that the enrichment process has completed.
        /// </summary>
        public void OnCompleted()
        {
            _subject.OnCompleted();
            _tcs.SetResult();
        }
        /// <summary>
        /// Notifies observers that an exception has occurred.
        /// </summary>
        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }
        /// <summary>
        /// Enriches the provided <see cref="IpHostContext"/> and notifies observers of the enriched context.
        /// </summary>
        public void OnNext(ObservableEvent<IpHostContext> value)
        {
            try
            {
                var tags = _tagProvider.GetTags(value);
                _subject.OnNext(new ObservableEvent<IpHostContextWithTags>(new IpHostContextWithTags { HostAddress = value.Payload.HostAddress, Flows = value.Payload.Flows, Tags = tags.ToArray() }, value.StartTime, value.EndTime));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in context enrichment.", value);
                _subject.OnError(ex);
            }

        }

        /// <summary>
        /// Subscribes an observer to the enriched context.
        /// </summary>
        /// <param name="observer">The observer that will receive notifications.</param>
        /// <returns>Disposable object representing the subscription.</returns>
        public IDisposable Subscribe(IObserver<ObservableEvent<IpHostContextWithTags>> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
