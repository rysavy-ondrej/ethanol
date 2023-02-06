namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Represents an extension data produced by SmartADS modules related to hosts.
    /// </summary>
    /// <param name="HostAddress">The host IP address.</param>
    /// <param name="Source">The source of the information, which determines also its meaning.</param>
    /// <param name="reliability">The reliability score of the information.</param>
    /// <param name="Value">The value of the extension data.</param>
    public record HostMetadata(string HostAddress, string Source, double reliability, string Value); 
}
