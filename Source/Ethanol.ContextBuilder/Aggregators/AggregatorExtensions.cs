using Ethanol.ContextBuilder.Aggregators;
using System;

namespace Ethanol.ContextBuilder.Observable
{

    /// <summary>
    /// Provides extension methods for aggregating event streams into time-based windows.
    /// </summary>
    public static class AggregatorExtensions
    {
        /// <summary>
        /// Transforms an observable sequence into a series of "hopping" windows, each containing events that
        /// occurred within a specified time span. Each window is represented as an observable sequence itself.
        /// This is useful for batch processing or summarizing data over fixed time intervals.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source observable sequence.</typeparam>
        /// <param name="source">The source observable sequence that will be divided into windows.</param>
        /// <param name="timeSpan">The duration of each window. Events within this time span will be grouped together.</param>
        /// <returns>
        /// An observable sequence of windows (each represented as an observable sequence of items) that are 
        /// emitted at regular intervals defined by the specified duration.
        /// </returns>
        /// <remarks>
        /// Each window is closed at the end of its lifetime, meaning that once the specified time span for
        /// a window is over, it is emitted and a new window begins, even if the previous window is empty.
        /// This operation is stateful and must keep a memory of the events during the window time span.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the source observable is null.</exception>

        public static IObservable<ObservableEvent<IObservable<T>>> HoppingWindow<T>(this IObservable<ObservableEvent<T>> source, TimeSpan timeSpan)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return System.Reactive.Linq.Observable.Create<ObservableEvent<IObservable<T>>>(observer =>
            {
                var window = new HoppingWindowAggregator<T>(timeSpan);
                source.Subscribe(window);
                return window.Subscribe(observer);
            });
        }
    }
}
