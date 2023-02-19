﻿namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Represents an extension data produced by SmartADS modules related to hosts.
    /// </summary>
    /// <param name="HostAddress">The host IP address.</param>
    /// <param name="Source">The source of the information, which determines also its meaning.</param>
    /// <param name="Reliability">The reliability score of the information.</param>
    /// <param name="Value">The value of the extension data.</param>
    public record HostMetadata(string HostAddress, string Name, double Reliability, string Value);
}
