using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;

namespace Ethanol.Demo
{
    [ArtifactName("Dns")]
    public class ArtifactDns : IpfixArtifact
    {
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
    }
}