using AutoMapper;
using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;

namespace Ethanol.Demo
{
    [ArtifactName("Http")]
    [AutoMap(typeof(RawIpfixRecord))]
    public class ArtifactHttp : IpfixArtifact
    {     
        [Name("hhost")]
        public string HttpHostName { get; set; }
        [Name("hurl")]
        public string HttpUrl { get; set; }
    }
}