using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection.Metadata.Ecma335;

namespace Ethanol.Demo
{
    /// <summary>
    /// This corresponds to Flowmon "long" template.
    /// </summary>
    [ArtifactName("Flow")]
    public class ArtifactLong : IpfixArtifact
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
        public int Flows { get; set; }
    }    
}