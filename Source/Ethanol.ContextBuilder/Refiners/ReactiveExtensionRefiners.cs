using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Refiners
{
    public static class ReactiveExtensionRefiners
    {
        public static IObservable<R?> Refine<T, R>(this IObservable<T> source, IRefiner<T, R> refiner)
        {
            return source.Select(item => refiner.Refine(item));
        }

        
        public static IObservable<R?> Refine<T, R>(this IObservable<T> source, IRefiner<T, R> refiner, IScheduler scheduler, int maxConcurrent = 4)
        {
            return source.Select(item =>
                Observable.Start(() =>
                {
                    return refiner.Refine(item);
                }, scheduler))
            .Merge(maxConcurrent);
        }

        public static IObservable<R?> Refine<T, R>(this IObservable<T> source, Func<T, R?> refiner)
        {
            return source.Select(item => refiner(item));
        }
    }
}
