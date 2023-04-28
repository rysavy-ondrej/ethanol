using Ethanol.ContextBuilder.Context;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.ContextBuilder.Enrichers
{
    public record FlowTag(DateTime StartTime, DateTime EndTime, string LocalAddress, ushort LocalPort, string RemoteAddress, ushort RemotePort, string ProcessName);

    /// <summary>
    /// Represents a record for enriched host contexts.
    /// </summary>
    /// <param name="HostAddress">The host address, which stands for the key of the record.</param>
    /// <param name="Flows">The array of associated flows.</param>
    /// <param name="HostTags">The array of metadata related to the host.</param>
    public record IpRichHostContext(IPAddress HostAddress, IpFlow[] Flows, HostTag[] HostTags, FlowTag[] FlowTags, IDictionary<string,NetifyTag[]> WebApps);
}
