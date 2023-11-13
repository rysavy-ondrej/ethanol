using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a no-op context enricher that does not modify or enrich the incoming data.
    /// This class is intended for use in situations where a placeholder or no-action operation is required in a processing pipeline.
    /// </summary>

    public class VoidContextEnricher<TSource, TTarget> : IObservableTransformer<TSource, TTarget>
    {
        private readonly Func<TSource, TTarget> _transform;

        /// <summary>
        /// Underlying subject that relays observed items to subscribers.
        /// </summary>
        private Subject<TTarget> _subject = new Subject<TTarget>();

        /// Represents a mechanism for signaling the completion of some asynchronous operation. 
        /// This provides a way to manually control the lifetime of a Task, signaling its completion.
        private TaskCompletionSource _tcs = new TaskCompletionSource();

        public VoidContextEnricher(Func<TSource, TTarget> transform)
        {

            this._transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public Task Completed => _tcs.Task;

        public void OnCompleted()
        {
            _subject.OnCompleted();
            _tcs.SetResult();
        }

        public void OnError(Exception error)
        {
            _subject.OnError(error); 
        }

        public void OnNext(TSource value)
        {
            _subject.OnNext(_transform(value));
        }

        public IDisposable Subscribe(IObserver<TTarget> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
