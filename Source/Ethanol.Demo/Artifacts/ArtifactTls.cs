using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ethanol.Demo
{

    [ArtifactName("Tls")]
    public class ArtifactTls : IpfixArtifact
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
        public string TlsClientVersion { get; set; }
        
        [Index(8)]
        public string TlsServerVersion { get; set; }
        
        [Index(9)]
        public string TlsServerCipherSuite { get; set; }
        
        [Index(10)]
        public string TlsServerName { get; set; }
        
        [Index(11)]
        public string TlsSubjectOrganizationName { get; set; }
        
        [Index(12)]
        public int Packets { get; set; }
        
        [Index(13)]
        public long Bytes { get; set; }
        /*
      public override IEnumerable<FactBuilder> Builders
      {
          get
          {
              if (this.DstPt == 443)
              {   // this is HTTPS
                  return new FactBuilder[] { 
                      FactLoaders.Common.ServiceDomainName, 
                      FactLoaders.Common.AdjacentFlow<ArtifactTls>(TimeSpan.FromMinutes(5)), 
                      FactLoaders.Tls.ReverseFlow, 
                      FactLoaders.Tls.SiblingFlow,
                      FactLoaders.Http.Related };
              }
              else
              {
                  return new FactBuilder[] { 
                      FactLoaders.Common.ServiceDomainName,
                      FactLoaders.Common.AdjacentFlow<ArtifactTls>(TimeSpan.FromMinutes(5)), 
                      FactLoaders.Tls.ReverseFlow };
             }
          }
      }
      */
    }
}