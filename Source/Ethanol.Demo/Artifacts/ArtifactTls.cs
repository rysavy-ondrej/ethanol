using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ethanol.Demo
{

    [ArtifactName("Tls")]
    public class ArtifactTls : Artifact
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


        #region Artifact overrides
        public override DateTime Start => DateTime.TryParse(FirstSeen, out var d) ? d : DateTime.MinValue;

        public override IPAddress Source => IPAddress.TryParse(SrcIp, out var x) ? x : null;

        public override IPAddress Destination => IPAddress.TryParse(DstIp, out var x) ? x : null;

        public override TimeSpan Duration => TimeSpan.TryParse(FlowDuration, out var d) ? d : TimeSpan.Zero;

        #endregion

        public override IEnumerable<FactBuilder> Builders
        {
            get
            {
                if (this.DstPt == 443)
                {   // this is HTTPS
                    return new FactBuilder[] { 
                        FactLoaders.Common.DomainName, 
                        FactLoaders.Common.Adjacent<ArtifactTls>(TimeSpan.FromMinutes(5)), 
                        FactLoaders.Tls.Reverse, 
                        FactLoaders.Tls.PossiblyRelatedTls,
                        FactLoaders.Http.Related };
                }
                else
                {
                    return new FactBuilder[] { 
                        FactLoaders.Common.DomainName,
                        FactLoaders.Common.Adjacent<ArtifactTls>(TimeSpan.FromMinutes(5)), 
                        FactLoaders.Tls.Reverse };
               }
            }
        }
    }
}