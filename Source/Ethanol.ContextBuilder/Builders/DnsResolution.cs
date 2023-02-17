using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    ///  Represents selected information from DNS connection.
    /// </summary>
    public record DnsResolution
    {
        public FlowKey Flow { get; set; }
        public string DomainNane { get; set; }
        public string[] Addresses { get; set; }
    }
}
