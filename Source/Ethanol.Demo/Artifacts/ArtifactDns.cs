using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;

namespace Ethanol.Demo
{
    [ArtifactName("Dns")]
    public class ArtifactDns : IpfixArtifact
    {
        [Name("ts")]
        public override string FirstSeen { get; set; }

        [Name("sa")]
        public override string SrcIp { get; set; }

        [Name("da")]
        public override string DstIp { get; set; }

        [Name("dnsflags")]
        public string DnsFlag { get; set; }

        [Name("dnsqtype")]
        public string DnsQuestionType { get; set; }

        [Name("dnsqname")]
        public string DnsQuestionName { get; set; }

        [Name("dnsrname")]
        public string DnsResponseName { get; set; }

        [Name("dnsrdata")]
        public string DnsResponseData { get; set; }

        [Name("dnsrcode")]
        public string DnsResponseCode { get; set; }

        [Name("pkt")]
        public int Packets { get; set; }

        [Name("byt")]
        public long Bytes { get; set; }

        [Ignore]
        public override string Duration { get; set; }
        [Ignore]
        public override int SrcPt { get; set; }
        [Ignore]
        public override int DstPt { get; set; }
        [Ignore]
        public override string Protocol { get; set; } = "UDP";
    }
}