using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol.Streaming
{
    public static class StreamableObservables
    {
        /// <summary>
        /// Gets the stream from the given observable considering each records a point event. 
        /// It uses <paramref name="getStartTime"/> to retrieve 
        /// start time of observable records to produce stream events. It also performs 
        /// hopping window time adjustement on all ingested events.
        /// </summary>
        /// <typeparam name="T">The type of event payloads.</typeparam>
        /// <param name="observable">The input observable.</param>
        /// <param name="getStartTime">The function to get start time of a record.</param>
        /// <param name="windowSize">Size of the hopping window.</param>
        /// <param name="windowHop">Hop size in ticks.</param>
        /// <returns>A stream of events with defined start times adjusted to hopping windows.</returns>
        public static IStreamable<Empty, T> GetWindowedEventStream<T>(this IObservable<T> observable, Func<T, long> getStartTime, TimeSpan windowSize, TimeSpan windowHop)
        {
            static bool ValidateTimestamp((T Record, long StartTime) record) =>
                record.StartTime > DateTime.MinValue.Ticks && record.StartTime < DateTime.MaxValue.Ticks;

            var source = observable
                .Select(x => (Record: x, StartTime: getStartTime(x)))
                .Where(ValidateTimestamp)
                .Select(x => StreamEvent.CreatePoint(x.StartTime, x.Record));
            return source.ToStreamable(disorderPolicy: DisorderPolicy.Adjust(TimeSpan.FromMinutes(5).Ticks), FlushPolicy.FlushOnBatchBoundary).HoppingWindowLifetime(windowSize.Ticks, windowHop.Ticks);
        }
    }
}
