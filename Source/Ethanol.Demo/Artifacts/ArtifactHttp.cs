using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [ArtifactName("Http")]
    public class ArtifactHttp : Artifact
    {
              
        [Index(0)]
        public string FirstSeen { get; set; }
        [Index(1)]
        public string FlowDuration { get; set; }
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
        public override DateTime Start => DateTime.TryParse(FirstSeen, out var d) ? d : DateTime.MinValue;

        public override IPAddress Source => IPAddress.TryParse(SrcIp, out var x) ? x : null;

        public override IPAddress Destination => IPAddress.TryParse(DstIp, out var x) ? x : null;

        public override TimeSpan Duration => TimeSpan.TryParse(FlowDuration, out var d) ? d : TimeSpan.Zero;

        public override IEnumerable<FactBuilder> Builders => new FactBuilder[] { FactLoaders.Common.ServiceDomainName, FactLoaders.Common.AdjacentFlow<ArtifactHttp>(TimeSpan.FromMinutes(10)) };
    }
}