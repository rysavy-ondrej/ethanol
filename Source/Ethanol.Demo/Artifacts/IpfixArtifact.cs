using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ethanol.Demo
{
    /// <summary>
    /// Base class for all IPFIX artifacts.
    /// </summary>
    public abstract class IpfixArtifact : Artifact
    {
        /// <summary>
        /// Timestamp of the first packet of the flow.
        /// </summary>
        public abstract string FirstSeen { get; set; }
        /// <summary>
        /// Duration of the event related to the artifact, e.g., duration of the flow.
        /// </summary>
        public abstract string Duration { get; set; }
        /// <summary>
        /// Source IP address of the flow object.
        /// </summary>
        public abstract string SrcIp { get; set;  }
        /// <summary>
        /// Destionation IP address of the flow object.
        /// </summary>
        public abstract string DstIp { get; set;  }

        public abstract int SrcPt { get; set; }

        public abstract int DstPt { get; set; }

        public abstract string Protocol { get; set; }
    }
}