using CsvHelper.Configuration.Attributes;
using System;

namespace Ethanol.Console.DataObjects
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
        public FlowKey FlowKey => new FlowKey { Proto = "TCP", SrcIp = LocalAddress, SrcPt = LocalPort, DstIp = RemoteAddress, DstPt = RemotePort };

        [Name("LocalAddress")]
        public string LocalAddress { get; set; }

        [Name("LocalPort")]
        public int LocalPort { get; set; }

        [Name("RemoteAddress")]
        public string RemoteAddress { get; set; }

        [Name("RemotePort")]
        public int RemotePort { get; set; }

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



