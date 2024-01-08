using System;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Enrichers
{
    public static class ReactiveExtensionsEnrichers
    {
        public static IObservable<R> Enrich<T, R>(this IObservable<T> source, IEnricher<T, R> enricher)
        {
            return source.Select(item => enricher.Enrich(item));
        }
        public static IObservable<R> Enrich<T, R>(this IObservable<T> source, Func<T, R> enricher)
        {
            return source.Select(item => enricher(item));
        }
    }
}
