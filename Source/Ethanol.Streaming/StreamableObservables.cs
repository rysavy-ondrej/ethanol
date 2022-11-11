using Microsoft.StreamProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// <param name="windowSize">SWindow size.</param>
        /// <param name="windowPeriod">Window period.</param>
        /// <returns>A stream of events with defined start times adjusted to hopping windows.</returns>
        public static IStreamable<Empty, T> GetWindowedEventStream<T>(this IObservable<T> observable, Func<T, long> getStartTime, TimeSpan windowSize, TimeSpan windowPeriod)
        {
            
            var source = observable.Select(x => StreamEvent.CreatePoint(RoundMinutes(getStartTime(x), windowPeriod.Ticks).Ticks, x));
            var stream = source.ToStreamable(disorderPolicy: DisorderPolicy.Adjust(windowSize.Ticks), FlushPolicy.FlushOnPunctuation);

            return stream.AlterEventDuration(windowPeriod.Ticks);
            //return stream.QuantizeLifetime(windowSize.Ticks, windowPeriod.Ticks);
        }
        private static DateTime RoundMinutes(long arg, long roundTicks)
        {            
            var sub = arg % roundTicks;
            var newDt = new DateTime(arg - sub);
            return newDt;
        }
    }
}
