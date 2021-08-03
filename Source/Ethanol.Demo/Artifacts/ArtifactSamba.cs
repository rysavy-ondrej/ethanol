using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [ArtifactName("Samba")]
    public class ArtifactSamba : Artifact
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
        public string Smb1Command { get; set; }

        [Index(10)]
        public string Smb2Command { get; set; }
        [Index(11)]
        public string SmbOperation { get; set; }
        [Index(12)]
        public string SmbFileType { get; set; }

        [Index(13)]
        public string SmbTreePath { get; set; }
        [Index(14)]
        public string SmbFilePath { get; set; }
        [Index(15)]
        public int Packets { get; set; }
        [Index(16)]
        public int Bytes { get; set; }
        [Index(17)]
        public int PacketPerSecond { get; set; }
        [Index(18)]
        public int BytesPerSeconds { get; set; }
        [Index(19)]
        public int BytesPerPacket { get; set; }
        [Index(20)]
        public int Flows { get; set; }

        public override DateTime Start => DateTime.TryParse(FirstSeen, out var d) ? d : DateTime.MinValue;

        public override IPAddress Source => IPAddress.TryParse(SrcIp, out var x) ? x : null;

        public override IPAddress Destination => IPAddress.TryParse(DstIp, out var x) ? x : null;

        public override TimeSpan Duration => TimeSpan.TryParse(FlowDuration, out var d) ? d : TimeSpan.Zero;

        public override IEnumerable<FactBuilder> Builders => new FactBuilder[] { FactLoaders.Common.ServiceDomainName, FactLoaders.Common.AdjacentFlow<ArtifactSamba>(TimeSpan.FromMinutes(10)) };
    }
}