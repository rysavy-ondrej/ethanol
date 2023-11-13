using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Provides extension methods for working with <see cref="IpFlow"/> objects.
    /// </summary>
    public static class IpFlowExtensions
    {
        /// <summary>
        /// Filters the flows of a specific type and projects them into a new form.
        /// </summary>
        /// <typeparam name="T">The specific type of <see cref="IpFlow"/> to filter on.</typeparam>
        /// <typeparam name="R">The type of the value returned by the <paramref name="func"/>.</typeparam>
        /// <param name="flows">The sequence of <see cref="IpFlow"/> to filter and project.</param>
        /// <param name="func">A transform function to apply to each flow of type <typeparamref name="T"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of projected flows.</returns>
        public static IEnumerable<R> SelectFlows<T, R>(this IEnumerable<IpFlow> flows, Func<T, R> func) where T : IpFlow
        {
            return flows.Where(x => x is T).Cast<T>().Select(func);
        }

        /// <summary>
        /// Filters the flows to those of a specific type.
        /// </summary>
        /// <typeparam name="T">The specific type of <see cref="IpFlow"/> to filter on.</typeparam>
        /// <param name="flows">The sequence of <see cref="IpFlow"/> to filter.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing only flows of type <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> SelectFlows<T>(this IEnumerable<IpFlow> flows) where T : IpFlow
        {
            return flows.Where(x => x is T).Cast<T>();
        }

        /// <summary>
        /// Determines the remote address in an IP flow relative to a provided host address.
        /// </summary>
        /// <param name="flow">The <see cref="IpFlow"/> for which the remote address needs to be determined.</param>
        /// <param name="host">The local host's <see cref="IPAddress"/> to compare against the flow's source and destination.</param>
        /// <returns>The remote <see cref="IPAddress"/> that corresponds to the provided host address in the flow.</returns>
        public static IPAddress GetRemoteAddress(this IpFlow flow, IPAddress host)
        {
            return host.Equals(flow.SourceAddress) ? flow.DestinationAddress : flow.SourceAddress;
        }
    }


}
