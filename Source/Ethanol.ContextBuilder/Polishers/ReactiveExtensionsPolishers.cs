using System;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Polishers
{
    public static class ReactiveExtensionsPolishers
    {
        public static IObservable<R> Polish<T, R>(this IObservable<T> source, IRefiner<T, R> polisher)
        {
            return source.Select(item => polisher.Refine(item));
        }
        public static IObservable<R> Polish<T, R>(this IObservable<T> source, Func<T, R> polisher)
        {
            return source.Select(item => polisher(item));
        }
    }
}
