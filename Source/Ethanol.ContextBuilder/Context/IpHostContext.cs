using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Represents the context for a specific IP host, encompassing its address, associated flows, and optional custom data.
    /// </summary>
    /// <typeparam name="CustomData">Type of custom data associated with the IP host context.</typeparam>
    public class IpHostContext<CustomData>
    {
        /// <summary>
        /// Gets the IP address of the host.
        /// </summary>
        public IPAddress HostAddress { get; init; }

        /// <summary>
        /// Gets the collection of flows associated with the host.
        /// </summary>
        public IpFlow[] Flows { get; init; }

        /// <summary>
        /// Gets the custom data associated with the host, represented by the specified type <typeparamref name="CustomData"/>.
        /// </summary>
        public CustomData Tags { get; init; }

        /// <summary>
        /// Gets flows of type <typeparamref name="TFlow"/> using the provided <paramref name="select"/> function. 
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <typeparam name="TFlow">The type of flows to retrieve.</typeparam>
        /// <param name="select">The result mapping function.</param>
        /// <returns>An enumeration of flow objects created using the provided <paramref name="select"/> function.</returns>
        public IEnumerable<TResult> GetFlowsAs<TResult, TFlow>(Func<TFlow, TResult> select) where TFlow : IpFlow
            => Flows.Where(f => f is TFlow).Select(f => select(f as TFlow));
    }

    /// <summary>
    /// Represents the context for a specific IP host without any associated custom data.
    /// </summary>
    public class IpHostContext : IpHostContext<Empty>
    {
    }
}
