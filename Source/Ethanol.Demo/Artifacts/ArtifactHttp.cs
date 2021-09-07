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
        [Name("ts")]
        public override  string FirstSeen { get; set; }
        [Name("td")]
        public override string Duration { get; set; }
        [Name("sa")]
        public override string SrcIp { get; set; }
        [Name("da")]
        public override  string DstIp { get; set; }
        [Name("hhost")]
        public string HttpHostName { get; set; }
        [Name("hurl")]
        public string HttpUrl { get; set; }
        [Name("dp")]
        public override int DstPt { get; set; }
        [Name("pkt")]
        public int Packets { get; set; }
        [Name("byt")]                                                                                                   
        public long Bytes { get; set; }

        [Ignore]
        public override int SrcPt { get; set; }
        [Ignore]
        public override string Protocol { get; set; } = "TCP";
    }
}