using Ethanol.ContextBuilder.Context;
using System.Net;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a record for enriched host contexts.
    /// </summary>
    /// <param name="HostAddress">The host address, which stands for the key of the record.</param>
    /// <param name="Flows">The array of associated flows.</param>
    /// <param name="Metadata">The array of metadata related to the host.</param>
    public record IpRichHostContext(IPAddress HostAddress, IpFlow[] Flows, HostTag[] Metadata);
}
