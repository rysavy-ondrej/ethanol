using System;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Represents a span of time window in which the context information is computed. 
    /// Validity of context flow is determined by this window.
    /// </summary>
    /// <param name="Start">The start of the window.</param>
    /// <param name="Duration">The duration of the window.</param>
    public record WindowSpan(DateTime Start, TimeSpan Duration)
    {
        internal static WindowSpan FromLong(long startTime, long endTime)
        {
            return new WindowSpan(new DateTime(startTime), new TimeSpan(endTime - startTime));
        }
    }
}
