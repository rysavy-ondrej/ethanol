using CsvHelper.Configuration.Attributes;
using Ethanol.Artifacts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection.Metadata.Ecma335;

namespace Ethanol.Demo
{
    /// <summary>
    /// This corresponds to Flowmon "long" template.
    /// </summary>
    [ArtifactName("Flow")]
    public class ArtifactLong : IpfixArtifact
    {        
        [Name("bpp")]
        public string BytesPerPacket { get; set; }

        [Name("pps")]
        public string PacketsPerSecond { get; set; }
    }    
}