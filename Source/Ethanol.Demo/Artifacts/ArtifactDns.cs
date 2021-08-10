using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{
    [ArtifactName("Dns")]
    public class ArtifactDns : IpfixArtifact
    {
        [Index(0)]
        public override string FirstSeen { get; set; }

        [Index(1)]
        public override string SrcIp { get; set; }

        [Index(2)]
        public override string DstIp { get; set; }

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

        [Ignore]
        public override string Duration { get; set; }
        [Ignore]
        public override int SrcPt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        [Ignore]
        public override int DstPt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}