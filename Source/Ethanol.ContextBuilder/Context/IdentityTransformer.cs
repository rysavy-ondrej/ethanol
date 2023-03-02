using System;
using System.Reactive.Subjects;

namespace Ethanol.ContextBuilder.Context
{
    public class IdentityTransformer<TSource> : IObservableTransformer<TSource, TSource>
    {
        Subject<TSource> _subject = new Subject<TSource>();

        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        public void OnNext(TSource value)
        {
            _subject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
