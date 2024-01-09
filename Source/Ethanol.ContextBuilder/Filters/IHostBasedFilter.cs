using Ethanol.DataObjects;
using System.Net;

namespace Ethanol.ContextBuilder.Pipeline
{
    /// <summary>
    /// Represents a filter based on the host of an event.
    /// </summary>
    public interface IHostBasedFilter
    {
        /// <summary>
        /// Evaluates the specified event against the filter.
        /// </summary>
        /// <param name="evt">The event to evaluate.</param>
        /// <returns>True if the event passes the filter; otherwise, false.</returns>
        bool Evaluate(TimeRange<IpHostContext> evt);

        /// <summary>
        /// Determines if the specified IP address matches the filter.
        /// </summary>
        /// <param name="address">The IP address to match.</param>
        /// <returns>True if the IP address matches the filter; otherwise, false.</returns>
        bool Match(IPAddress address);
    }
}