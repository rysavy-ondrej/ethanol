using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [ArtifactName("Tcp")]
    public class ArtifactTcp : IpfixArtifact
    {
        [Index(0)]
        public override string FirstSeen { get; set; }
        [Index(1)]
        public override string Duration { get; set; }
        [Index(2)]
        public override string Protocol { get; set; }

        [Index(3)]
        public override string SrcIp { get; set; }
        [Index(4)]
        public override int SrcPt { get; set; }

        [Index(5)]
        public override string DstIp { get; set; }
        [Index(6)]
        public override int DstPt { get; set; }

        [Index(7)]
        public string Flags { get; set; }

        [Index(8)]
        public int Tos { get; set; }
        [Index(9)]
        public int Packets { get; set; }
        [Index(10)]
        public int Bytes { get; set; }
        [Index(11)]
        public int PacketPerSecond { get; set; }
        [Index(12)]
        public int BytesPerSeconds { get; set; }
        [Index(13)]
        public int BytesPerPacket { get; set; }
        [Index(14)]
        public int Flows { get; set; }
    }
}