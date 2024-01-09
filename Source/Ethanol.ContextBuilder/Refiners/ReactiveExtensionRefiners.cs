using System;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Refiners
{
    public static class ReactiveExtensionRefiners
    {
        public static IObservable<R?> Refine<T, R>(this IObservable<T> source, IRefiner<T, R> polisher)
        {
            return source.Select(item => polisher.Refine(item));
        }
        public static IObservable<R?> Refine<T, R>(this IObservable<T> source, Func<T, R?> polisher)
        {
            return source.Select(item => polisher(item));
        }
    }
}
