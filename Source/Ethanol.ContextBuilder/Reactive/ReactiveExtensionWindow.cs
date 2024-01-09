using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Schedulers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;


namespace Ethanol.ContextBuilder.Reactive
{

    public static class ReactiveExtensionWindow
    {
        /// <summary>
        /// Creates a virtual window over an observable sequence of timestamped values.
        /// </summary>
        /// <typeparam name="T">The type of the values in the observable sequence.</typeparam>
        /// <param name="source">The source observable sequence.</param>
        /// <param name="windowSize">The size of the virtual window.</param>
        /// <param name="windowShift">The shift of the virtual window.</param>
        /// <returns>An observable sequence of observable sequences of timestamped values representing the virtual windows.</returns>
        public static IObservable<TimeRange<IObservable<Timestamped<T>>>> VirtualWindow<T>(
            this IObservable<Timestamped<T>> source,
            TimeSpan windowSize,
            TimeSpan windowShift)
        {
            var virtualScheduler = new VirtualFlowTimeScheduler();
            var firstFlow = true;

            DateTimeOffset GetWindowStart(DateTimeOffset timestamp)
            {
                var timeOfDayTicks = timestamp.TimeOfDay.Ticks;
                var windowTicks = windowSize.Ticks % TimeSpan.FromDays(1).Ticks;   //this is for sure that we are within a single day
                var shiftTicks = timeOfDayTicks % windowTicks;
                var windowStart = timestamp - TimeSpan.FromTicks(shiftTicks);
                return windowStart;
            }

            void ShiftTime(Timestamped<T> flow)
            {
                if (firstFlow)
                {
                    var windowStart = GetWindowStart(flow.Timestamp);
                    virtualScheduler.SkipTo(windowStart);
                    firstFlow = false;
                }
                if (flow.Timestamp > virtualScheduler.Clock)
                {
                    virtualScheduler.AdvanceTo(flow.Timestamp);
                }
            }

            return System.Reactive.Linq.Observable.Create<TimeRange<IObservable<Timestamped<T>>>>(observer =>
            {
                return source
                    .Do(ShiftTime).Window(windowSize, windowShift, virtualScheduler).Timestamp(virtualScheduler)
                    .Select(w => new TimeRange<IObservable<Timestamped<T>>>(w.Value, w.Timestamp.Ticks, w.Timestamp.Ticks + windowSize.Ticks))
                    .Subscribe(observer);
            });
        }

        /// <summary>
        /// Aggregates the elements of an observable sequence using given key selector function.
        /// Creates Observable events with the given time window.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source observable sequence.</typeparam>
        /// <typeparam name="R">The type of the result.</typeparam>
        /// <typeparam name="K">The type of the key.</typeparam>
        /// <typeparam name="V">The type of the value.</typeparam>
        /// <param name="source">The source observable sequence.</param>
        /// <param name="windowStartTime">The start time of the time window.</param>
        /// <param name="windowEndTime">The end time of the time window.</param>
        /// <param name="keyValSelector">A function that maps each element to an array of key-value pairs.</param>
        /// <param name="resultSelector">A function to aggregate the values for each key into a result.</param>
        /// <param name="emptyResultSelector">A function to generate an empty result.</param>
        /// <returns>An observable sequence of aggregated results within the specified time window.</returns>
        public static IObservable<TimeRange<R>> Aggregate<T, R, K, V>(
                this IObservable<Timestamped<T>> source,
                long windowStartTime, long windowEndTime,
                Func<Timestamped<T>, KeyValuePair<K, V?>[]> keyValSelector,
                Func<KeyValuePair<K, V?[]>, R> resultSelector,
                Func<R> emptyResultSelector
        )
        {
            return source
                    .SelectMany(keyValSelector)
                    .GroupByAggregate(k => k.Key, v => v.Value, g => new TimeRange<R>(resultSelector(g), windowStartTime, windowEndTime))
                    .Append(new TimeRange<R>(emptyResultSelector(), windowStartTime, windowEndTime));

        }

        /// <summary>
        /// Aggregates the IP flows within a specified time window and returns an observable sequence of observable events containing the aggregated IP host contexts.
        /// </summary>
        /// <param name="flows">The observable sequence of timestamped IP flows.</param>
        /// <param name="windowStartTime">The start time of the window.</param>
        /// <param name="windowEndTime">The end time of the window.</param>
        /// <returns>An observable sequence of observable events containing the aggregated IP host contexts.</returns>
        public static IObservable<TimeRange<IpHostContext>> AggregateIpContexts(this IObservable<Timestamped<IpFlow>> flows, long windowStartTime, long windowEndTime)
        {
            return flows.Aggregate(
                windowStartTime, windowEndTime,
                bihostFlowKeySelector,
                g => new IpHostContext { HostAddress = g.Key, Flows = g.Value.Where(f => f != null).Select(f => f!).ToArray() },
                () => new IpHostContext { HostAddress = IPAddress.Any, Flows = Array.Empty<IpFlow>() }
            );
        }

        /// <summary>
        /// Duplicates the given IP flow by creating two key-value pairs, one with the source address and one with the destination address.
        /// </summary>
        /// <param name="flow">The IP flow to duplicate.</param>
        /// <returns>An array of key-value pairs, where the key is the IP address and the value is the duplicated IP flow.</returns>
        static KeyValuePair<IPAddress, IpFlow?>[] bihostFlowKeySelector(Timestamped<IpFlow> flow)
        {
            return new[] {
                    new KeyValuePair<IPAddress, IpFlow?>(flow.Value?.SourceAddress ?? IPAddress.None, flow.Value),
                    new KeyValuePair<IPAddress, IpFlow?>(flow.Value?.DestinationAddress ?? IPAddress.None, flow.Value) };
        }
    }
}