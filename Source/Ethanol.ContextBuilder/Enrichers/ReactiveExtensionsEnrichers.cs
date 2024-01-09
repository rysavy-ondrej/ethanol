using System;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Enrichers
{
    public static class ReactiveExtensionsEnrichers
    {
        /// <summary>
        /// Enriches an observable sequence by applying an enricher to each item.
        /// </summary>
        /// <typeparam name="T">The type of the items in the source observable sequence.</typeparam>
        /// <typeparam name="R">The type of the enriched items.</typeparam>
        /// <param name="source">The source observable sequence.</param>
        /// <param name="enricher">The enricher to apply to each item.</param>
        /// <returns>An observable sequence of enriched items.</returns>
        public static IObservable<R> Enrich<T, R>(this IObservable<T> source, IEnricher<T, R> enricher)
        {
            return source.Select(item => enricher.Enrich(item));
        }
        /// <summary>
        /// Enriches an observable sequence by applying a function to each element.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="R">The type of the elements in the enriched sequence.</typeparam>
        /// <param name="source">The source observable sequence.</param>
        /// <param name="enricher">The function to apply to each element.</param>
        /// <returns>An observable sequence that contains the enriched elements.</returns>
        public static IObservable<R> Enrich<T, R>(this IObservable<T> source, Func<T, R> enricher)
        {
            return source.Select(item => enricher(item));
        }
    }
}
