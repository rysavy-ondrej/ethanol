
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
    public class IpConnectionInfo
    {
        public string? RemoteHostAddress { get; set; }
        public string? RemoteHostName { get; set; }
        public ushort RemotePort { get; set; }
        public string? ApplicationProcessName { get; set; }
        public InternetServiceTag[]? InternetServices { get; set; }
        public int Flows { get; set; }
        public int PacketsSent { get; set; }
        public long OctetsSent { get; set; }
        public int PacketsRecv { get; set; }
        public long OctetsRecv { get; set; }

        public IpConnectionInfo(string remoteHostAddress, string remoteHostName, ushort remotePort, string applicationProcessName, InternetServiceTag[] internetServices, int flows, int packetsSent, long octetsSent, int packetsRecv, long octetsRecv)
        {
            RemoteHostAddress = remoteHostAddress;
            RemoteHostName = remoteHostName;
            RemotePort = remotePort;
            ApplicationProcessName = applicationProcessName;
            InternetServices = internetServices;
            Flows = flows;
            PacketsSent = packetsSent;
            OctetsSent = octetsSent;
            PacketsRecv = packetsRecv;
            OctetsRecv = octetsRecv;
        }

        public IpConnectionInfo()
        {
        }
    }
}
