using System.Net;


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
        public IPAddress? HostAddress { get; init; }

        /// <summary>
        /// Gets the collection of flows associated with the host.
        /// </summary>
        public IpFlow[]? Flows { get; init; }

        /// <summary>
        /// Gets the custom data associated with the host, represented by the specified type <typeparamref name="CustomData"/>.
        /// </summary>
        public CustomData? Tags { get; init; }
    }

    /// <summary>
    /// Represents the context for a specific IP host without any associated custom data.
    /// </summary>
    public class IpHostContext : IpHostContext<Empty>
    {
    }
}
