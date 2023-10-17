using System;

namespace Ethanol.ContextBuilder.Observable
{
    /// <summary>
    /// Provides windowing operators for observables.
    /// </summary>
    public static class WindowOperators
    {
        /// <summary>
        /// Creates a windowed observable from the provided source observable, where each window covers the specified duration.
        /// This operator enables the transformation of a time-based sequence into hopping windows.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source observable.</typeparam>
        /// <param name="source">The source observable to window.</param>
        /// <param name="timeSpan">The duration of each window.</param>
        /// <returns>An observable sequence of windows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source observable is null.</exception>
        public static IObservable<ObservableEvent<IObservable<T>>> HoppingWindow<T>(this IObservable<ObservableEvent<T>> source, TimeSpan timeSpan)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return System.Reactive.Linq.Observable.Create<ObservableEvent<IObservable<T>>>(observer =>
            {
                var window = new HoppingWindowImplementation<T>(timeSpan);
                source.Subscribe(window);
                return window.Subscribe(observer);
            });
        }
    }
}
