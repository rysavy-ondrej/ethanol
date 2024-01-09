using System;

namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents an observable event with a specified payload and a time range during which the event occurred.
    /// </summary>
    /// <typeparam name="T">The type of the data associated with the event.</typeparam>
    public record TimeRange<T>
    {
        /// <summary>
        /// Gets the start time of the event.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets the end time of the event.
        /// </summary>
        public DateTime EndTime { get; set;  }

        /// <summary>
        /// Gets the payload or data associated with the event.
        /// </summary>
        public T? Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeRange{TPayload}"/> record 
        /// using a payload and start and end times in tick format.
        /// </summary>
        /// <param name="payload">The payload or data of the event.</param>
        /// <param name="startTime">The start time of the event in ticks.</param>
        /// <param name="endTime">The end time of the event in ticks.</param>
        public TimeRange(T payload, long startTime, long endTime)
        {
            Value = payload;
            StartTime = new DateTime(startTime);
            EndTime = new DateTime(endTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeRange{TPayload}"/> record 
        /// using a payload and start and end times as DateTime values.
        /// </summary>
        /// <param name="payload">The payload or data of the event.</param>
        /// <param name="startTime">The start time of the event.</param>
        /// <param name="endTime">The end time of the event.</param>
        public TimeRange(T payload, DateTime startTime, DateTime endTime)
        {
            Value = payload;
            StartTime = startTime;
            EndTime = endTime;
        }

        /// <summary>
        /// Initializes an empty observable event.
        /// </summary>
        public TimeRange()
        {
        }
    }
}
