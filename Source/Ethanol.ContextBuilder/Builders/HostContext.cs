﻿using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Defines a context for a host.
    /// </summary>
    public class HostContext
    {
        /// <summary>
        /// An array of metadata related to the host. 
        /// </summary>
        public HostMetadata[] Metadata { get; set; }

        public NetworkActivity Activity { get; set; }
    }
}
