using System.Collections.Generic;
using System.Linq;
using System.Net;
using System;

namespace Ethanol.ContextBuilder.Context
{
    public class IpHostContext<CustomData>
    {
        public IPAddress HostAddress { get; init; }
        public IpFlow[] Flows { get; init; }
        public CustomData Tags { get; init; }
        /// <summary>
        /// Gets flows of type <typeparamref name="TFlow"/> using <paramref name="select"/> function. 
        /// </summary>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <typeparam name="TFlow">The type of flows to retrieve.</typeparam>
        /// <param name="select">The result mapping function.</param>
        /// <returns>Get the enumerablw of flow object created using <paramref name="select"/> funciton.</returns>
        public IEnumerable<TResult> GetFlowsAs<TResult, TFlow>(Func<TFlow, TResult> select) where TFlow : IpFlow
            => Flows.Where(f => f is TFlow).Select(f => select(f as TFlow));
    }

    public class IpHostContext : IpHostContext<Empty>
    {
    }
}
