﻿using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;

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
        public int Flows { get; set; }

        public override DateTime Start => DateTime.TryParse(FirstSeen, out var d) ? d : DateTime.MinValue;

        public override IPAddress Source => IPAddress.TryParse(SrcIp, out var x) ? x : null;

        public override IPAddress Destination => IPAddress.TryParse(DstIp, out var x) ? x : null;

        public override TimeSpan Duration => TimeSpan.TryParse(FlowDuration, out var d) ? d : TimeSpan.Zero;


        public override IEnumerable<FactBuilder> Builders => new FactBuilder[] { FactLoaders.Common.ServiceDomain, FactLoaders.Common.ÄdjacentFlow<ArtifactFlow>(TimeSpan.FromMinutes(10)) };
    }
}