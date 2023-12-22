
namespace Ethanol.DataObjects
{

    /// <summary>
    /// Represents information about an IP connection, including details on packet and flow counts.
    /// </summary>
    /// <param name="RemoteHostAddress">The remote host IP address.</param>
    /// <param name="RemoteHostName">The remote host name.</param>
    /// <param name="RemotePort">The remote port number.</param>
    /// <param name="Flows">The number of flows associated with the connection.</param>
    /// <param name="ApplicationProcessName">The name of the application or process that made the request or accepted the connection.</param>
    /// <param name="PacketsSent">The number of packets sent over the connection.</param>
    /// <param name="OctetsSent">The number of octets sent over the connection.</param>
    /// <param name="PacketsRecv">The number of packets received over the connection.</param>
    /// <param name="OctetsRecv">The number of octets received over the connection.</param>
    public record IpConnectionInfo(string RemoteHostAddress, string RemoteHostName, ushort RemotePort, string ApplicationProcessName, InternetServiceTag[] InternetServices, int Flows, int PacketsSent, long OctetsSent, int PacketsRecv, long OctetsRecv);
}
