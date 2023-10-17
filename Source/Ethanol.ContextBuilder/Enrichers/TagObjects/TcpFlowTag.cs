using System;

namespace Ethanol.ContextBuilder.Enrichers.TagObjects
{
    /// <summary>
    /// Represents a tcp flow of network traffic between two endpoints during a specified time interval.
    /// </summary>
    /// <param name="StartTime">The start time of the traffic flow.</param>
    /// <param name="EndTime">The end time of the traffic flow.</param>
    /// <param name="LocalAddress">The local IP address involved in the traffic flow.</param>
    /// <param name="LocalPort">The local port number involved in the traffic flow.</param>
    /// <param name="RemoteAddress">The remote IP address involved in the traffic flow.</param>
    /// <param name="RemotePort">The remote port number involved in the traffic flow.</param>
    /// <param name="ProcessName">The name of the process that initiated or received the traffic flow.</param>
    public record TcpFlowTag(
        DateTime StartTime,
        DateTime EndTime,
        string LocalAddress,
        int LocalPort,
        string RemoteAddress,
        int RemotePort,
        string ProcessName
    );

}
