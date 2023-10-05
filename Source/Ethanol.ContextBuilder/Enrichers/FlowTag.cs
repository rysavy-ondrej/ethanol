using System;

namespace Ethanol.ContextBuilder.Enrichers
{
    public record FlowTag(
        DateTime StartTime,
        DateTime EndTime,
        string Protocol,
        string LocalAddress,
        int LocalPort,
        string RemoteAddress,
        int RemotePort,
        string ProcessName
    );
}
