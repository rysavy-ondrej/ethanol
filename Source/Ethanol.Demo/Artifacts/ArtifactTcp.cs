﻿using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [ArtifactName("Tcp")]
    public class ArtifactTcp : Artifact
    {
        [Index(0)]
        public string FirstSeen { get; set; }
        [Index(1)]
        public string FlowDuration { get; set; }
        [Index(2)]
        public string Protocol { get; set; }

        [Index(3)]
        public string SrcIp { get; set; }
        [Index(4)]
        public int SrcPt { get; set; }

        [Index(5)]
        public string DstIp { get; set; }
        [Index(6)]
        public int DstPt { get; set; }

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

        public override DateTime Start => DateTime.TryParse(FirstSeen, out var d) ? d : DateTime.MinValue;

        public override IPAddress Source => IPAddress.TryParse(SrcIp, out var x) ? x : null;

        public override IPAddress Destination => IPAddress.TryParse(DstIp, out var x) ? x : null;

        public override TimeSpan Duration => TimeSpan.TryParse(FlowDuration, out var d) ? d : TimeSpan.Zero;

        public override IEnumerable<FactBuilder> Builders => new FactBuilder[] { FactLoaders.Common.DomainName, FactLoaders.Common.Surrounding<ArtifactTcp>(TimeSpan.FromMinutes(10)) };
    }
}