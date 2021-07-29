using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    /// <summary>
    /// This corresponds to Flowmon "long" template.
    /// </summary>
    [ArtifactName("Flow")]
    public class ArtifactFlow : Artifact
    {
        [Index(0)]
        public string FirstSeen { get; set; }
        [Index(1)]
        public string Duration { get; set; }
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
        public int Flows { get; set; }

        [Ignore]
        public DateTime FirstSeenDateTime => DateTime.TryParse(FirstSeen, out var d) ? d : DateTime.MinValue;
        [Ignore]
        public TimeSpan DurationTimeSpan => TimeSpan.TryParse(Duration, out var d) ? d : TimeSpan.MinValue;
        [Ignore]
        public IPAddress SrcIpAddress => IPAddress.TryParse(SrcIp, out var x) ? x : null;
        [Ignore]
        public IPAddress DstIpAddress => IPAddress.TryParse(DstIp, out var x) ? x : null;
    }
}