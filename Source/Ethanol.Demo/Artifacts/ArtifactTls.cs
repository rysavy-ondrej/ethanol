using AutoMapper;
using AutoMapper.Configuration.Annotations;
using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ethanol.Demo
{

    [ArtifactName("Tls")]
    [AutoMap(typeof(RawIpfixRecord))]
    public class ArtifactTls : IpfixArtifact
    {
        [SourceMember("tlscver")]
        [Name("tlscver")]
        public string TlsClientVersion { get; set; }
        [SourceMember("tlssver")]
        [Name("tlssver")]
        public string TlsServerVersion { get; set; }
        [SourceMember("tlsciph")]
        [Name("tlsciph")]
        public string TlsServerCipherSuite { get; set; }
        [SourceMember("tlssni")]
        [Name("tlssni")]
        public string TlsServerName { get; set; }
        [SourceMember("tlsscn")]
        [Name("tlsscn")]
        public string TlsSubjectCommonName { get; set; }
        [SourceMember("tlsson")]
        [Name("tlsson")]
        public string TlsSubjectOrganizationName { get; set; }
        [SourceMember("tlscont")]
        [Name("tlscont")]
        public string TlsContentType { get; set; }
        [SourceMember("tlsja3")]
        [Name("tlsja3")]
        public string Ja3Fingerprint { get; set; }

    }
}