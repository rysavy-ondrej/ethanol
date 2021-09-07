using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [ArtifactName("Tcp")]
    public class ArtifactTcp : IpfixArtifact
    {
        [Name("flg")]
        public string Flags { get; set; }

        [Name("pps")]
        public string PacketPerSecond { get; set; }
        [Name("bps")]
        public string BitsPerSeconds { get; set; }
        [Name("bpp")]
        public string BytesPerPacket { get; set; }

        [Name("tcpwinsize")]
        public string TcpWindowSize { get; set; }

        [Name("tcpsynsize")]
        public string TcpSynSize { get; set; }

        [Name("tcpttl")]
        public string TcpTimeToLive { get; set; }

    }
}