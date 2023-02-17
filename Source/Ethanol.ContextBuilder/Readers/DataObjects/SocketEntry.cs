using CsvHelper.Configuration.Attributes;
using Ethanol.ContextBuilder.Context;
using System;
using System.Net;

namespace Ethanol.ContextBuilder.Readers.DataObjects
{
    /// <summary>
    /// Represents a single record in the collection of TCP connection list
    /// produced on a local machine.
    /// </summary>
    public partial class SocketEntry
    {
        [Name("FlowKey")]
        public string FlowKeyString { get; set; }

        [Ignore]
        public FlowKey FlowKey => new FlowKey(System.Net.Sockets.ProtocolType.Tcp, LocalAddress, LocalPort, RemoteAddress, RemotePort);

        [Name("LocalAddress")]
        public IPAddress LocalAddress { get; set; }

        [Name("LocalPort")]
        public ushort LocalPort { get; set; }

        [Name("RemoteAddress")]
        public IPAddress RemoteAddress { get; set; }

        [Name("RemotePort")]
        public ushort RemotePort { get; set; }

        [Name("State")]
        public string State { get; set; }

        [Name("ProcessName")]
        public string ProcessName { get; set; }

        [Name("CreationTime")]
        public DateTime CreationTime { get; set; }

        [Name("CurrentTime")]
        public DateTime CurrentTime { get; set; }
    }
}



