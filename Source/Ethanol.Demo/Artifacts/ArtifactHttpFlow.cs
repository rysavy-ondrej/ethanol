using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [ArtifactName("Http")]
    public class ArtifactHttpFlow : Artifact
    {
              
        [Index(0)]
        public string FirstSeen { get; set; }
        [Index(1)]
        public string Duration { get; set; }
        [Index(2)]
        public string SrcIp { get; set; }
        [Index(3)]
        public string DstIp { get; set; }
        [Index(4)]
        public string HttpHostName { get; set; }
        [Index(5)]
        public string HttpUrl { get; set; }
        [Index(6)]
        public int DstPt { get; set; }
        [Index(7)]
        public int Packets { get; set; }
        [Index(8)]
        public long Bytes { get; set; }

        [Ignore]
        public DateTime FirstSeenDateTime => DateTime.TryParse(FirstSeen, out var d) ? d : DateTime.MinValue;
        [Ignore]
        public IPAddress SrcIpAddress => IPAddress.TryParse(SrcIp, out var x) ? x : null;
        [Ignore]
        public IPAddress DstIpAddress => IPAddress.TryParse(DstIp, out var x) ? x : null;
    }
}