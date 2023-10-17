using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Cleaners
{
    /// <summary>
    /// Represents a paired set of flows in the context of a network transaction.
    /// </summary>
    /// <typeparam name="TFlow">The specific type of the flow, which must inherit from <see cref="IpFlow"/>.</typeparam>
    /// <remarks>
    /// The record contains both a request flow (<see cref="ReqFlow"/>) and a corresponding response flow (<see cref="ResFlow"/>).
    /// It's designed to encapsulate the bidirectional communication between two endpoints.
    /// </remarks>
    public record FlowPair<TFlow>(TFlow ReqFlow, TFlow ResFlow) where TFlow : IpFlow;

}
