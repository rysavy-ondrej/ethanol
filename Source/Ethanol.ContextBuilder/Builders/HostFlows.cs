using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    ///  Represents a collection of flows related to the host.
    /// </summary>
    public record HostFlows
    {
        public string Host { get; set; }
        public InternetFlow[] Flows { get; set; }
    }
}
