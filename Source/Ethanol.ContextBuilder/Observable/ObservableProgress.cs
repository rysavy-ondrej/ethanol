using System;
using System.Reactive.Subjects;

namespace Ethanol.ContextBuilder.Observable
{
    /// <summary>
    /// Represents a progress observer and observable mechanism for tracking the progress of operations 
    /// and providing updates about the ongoing tasks.
    /// </summary>
    /// <typeparam name="T">The type of data that the progress mechanism operates on.</typeparam>
    internal class ObservableProgress<T> : IObservable<T>, IObserver<T>
    {
        private readonly Subject<T> _subject;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableProgress{T}"/> class.
        /// </summary>
        public ObservableProgress()
        {
            _subject = new Subject<T>();
        }

        /// <summary>
        /// Gets or sets an action to be executed when a new data object is received.
        /// </summary>
        public Action? OnObjectReceived { get; set; }

        /// <summary>
        /// Signals the end of the progress.
        /// </summary>
        public void OnCompleted()
        {
            ((IObserver<T>)_subject).OnCompleted();
        }

        /// <summary>
        /// Notifies the observer about an error that has occurred.
        /// </summary>
        /// <param name="error">The exception to provide to observers.</param>
        public void OnError(Exception error)
        {
            ((IObserver<T>)_subject).OnError(error);
        }

        /// <summary>
        /// Subscribes an observer to the observable progress updates.
        /// </summary>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns>A disposable object that allows unsubscribing from the updates.</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return ((IObservable<T>)_subject).Subscribe(observer);
        }

        /// <summary>
        /// Receives the next progress data and invokes the <see cref="OnObjectReceived"/> action.
        /// </summary>
        /// <param name="value">The progress data.</param>
        void IObserver<T>.OnNext(T value)
        {
            OnObjectReceived?.Invoke();
            ((IObserver<T>)_subject).OnNext(value);
        }
    }

}