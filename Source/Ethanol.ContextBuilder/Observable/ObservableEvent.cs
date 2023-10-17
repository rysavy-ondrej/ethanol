using System;

namespace Ethanol.ContextBuilder.Observable
{
    /// <summary>
    /// Represents an observable event with a specified payload and a time range during which the event occurred.
    /// </summary>
    /// <typeparam name="TPayload">The type of the data or payload associated with the event.</typeparam>
    public record ObservableEvent<TPayload>
    {
        /// <summary>
        /// Gets the start time of the event.
        /// </summary>
        public DateTime StartTime { get; init; }

        /// <summary>
        /// Gets the end time of the event.
        /// </summary>
        public DateTime EndTime { get; init; }

        /// <summary>
        /// Gets the payload or data associated with the event.
        /// </summary>
        public TPayload Payload { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableEvent{TPayload}"/> record 
        /// using a payload and start and end times in tick format.
        /// </summary>
        /// <param name="payload">The payload or data of the event.</param>
        /// <param name="startTime">The start time of the event in ticks.</param>
        /// <param name="endTime">The end time of the event in ticks.</param>
        public ObservableEvent(TPayload payload, long startTime, long endTime)
        {
            Payload = payload;
            StartTime = new DateTime(startTime);
            EndTime = new DateTime(endTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableEvent{TPayload}"/> record 
        /// using a payload and start and end times as DateTime values.
        /// </summary>
        /// <param name="payload">The payload or data of the event.</param>
        /// <param name="startTime">The start time of the event.</param>
        /// <param name="endTime">The end time of the event.</param>
        public ObservableEvent(TPayload payload, DateTime startTime, DateTime endTime)
        {
            Payload = payload;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
