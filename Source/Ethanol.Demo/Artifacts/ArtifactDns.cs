using AutoMapper.Configuration.Annotations;
using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;

namespace Ethanol.Demo
{
    [ArtifactName("Dns")]
    public class ArtifactDns : IpfixArtifact
    {
        [SourceMember("dnsflags")]
        [Name("dnsflags")]
        public string DnsFlag { get; set; }

        [SourceMember("dnsqtype")]
        [Name("dnsqtype")]
        public string DnsQuestionType { get; set; }

        [SourceMember("dnsqname")]
        [Name("dnsqname")]
        public string DnsQuestionName { get; set; }

        [SourceMember("dnsrname")]
        [Name("dnsrname")]
        public string DnsResponseName { get; set; }

        [SourceMember("dnsrdata")]
        [Name("dnsrdata")]
        public string DnsResponseData { get; set; }

        [SourceMember("dnsrcode")]
        [Name("dnsrcode")]
        public string DnsResponseCode { get; set; }
    }
}