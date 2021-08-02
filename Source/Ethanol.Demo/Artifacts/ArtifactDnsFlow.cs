using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [ArtifactName("Dns")]
    public class ArtifactDnsFlow : Artifact
    {
        [Index(0)]
        public string FirstSeen { get; set; }

        [Index(1)]
        public string SrcIp { get; set; }

        [Index(2)]
        public string DstIp { get; set; }

        [Index(3)]
        public string DnsFlag { get; set; }

        [Index(4)]
        public string DnsQuestionType { get; set; }

        [Index(5)]
        public string DnsQuestionName { get; set; }

        [Index(6)]
        public string DnsResponseName { get; set; }

        [Index(7)]
        public string DnsResponseData { get; set; }

        [Index(8)]
        public string DnsResponseCode { get; set; }

        [Index(9)]
        public int Packets { get; set; }

        [Index(10)]
        public long Bytes { get; set; }

        public override DateTime Start => DateTime.TryParse(FirstSeen, out var d) ? d : DateTime.MinValue;

        public override IPAddress Source => IPAddress.TryParse(SrcIp, out var x) ? x : null;

        public override IPAddress Destination => IPAddress.TryParse(DstIp, out var x) ? x : null;

        public override TimeSpan Duration => TimeSpan.Zero;

    }
}