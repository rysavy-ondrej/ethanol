using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [ArtifactName("Http")]
    public class ArtifactHttp : IpfixArtifact
    {
              
        [Index(0)]
        public override  string FirstSeen { get; set; }
        [Index(1)]
        public override string Duration { get; set; }
        [Index(2)]
        public override string SrcIp { get; set; }
        [Index(3)]
        public override  string DstIp { get; set; }
        [Index(4)]
        public string HttpHostName { get; set; }
        [Index(5)]
        public string HttpUrl { get; set; }
        [Index(6)]
        public override int DstPt { get; set; }
        [Index(7)]
        public int Packets { get; set; }
        [Index(8)]                                                                                                   
        public long Bytes { get; set; }

        [Ignore]
        public override int SrcPt { get; set; }
        [Ignore]
        public override string Protocol { get; set; } = "TCP";
    }
}