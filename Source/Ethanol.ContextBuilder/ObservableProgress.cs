using System;
using System.Reactive.Subjects;

namespace Ethanol.ContextBuilder
{
    internal class ObservableProgress<T> : IObservable<T>, IObserver<T>
    {
        Subject<T> _subject;
        public ObservableProgress()
        {
            _subject = new Subject<T>();
        }

        public Action OnObjectReceived { get; set; }

        public void OnCompleted()
        {
            ((IObserver<T>)_subject).OnCompleted();
        }

        public void OnError(Exception error)
        {
            ((IObserver<T>)_subject).OnError(error);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return ((IObservable<T>)_subject).Subscribe(observer);
        }

        void IObserver<T>.OnNext(T value)
        {
            OnObjectReceived?.Invoke();
            ((IObserver<T>)_subject).OnNext(value);
        }
    }
}