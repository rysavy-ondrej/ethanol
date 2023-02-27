namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a host tag object.
    /// <para/>
    /// The host metadata can come from the environment (eg. Smart ADS) or can be computed
    /// in the evaluator.
    /// </summary>
    /// <param name="HostAddress">The host identification (domain name or IP address).</param>
    /// <param name="Source">The source of the information, which determines also its meaning.</param>
    /// <param name="Reliability">The reliability score of the information.</param>
    /// <param name="Value">The value of the extension data.</param>
    public record HostTag(string HostAddress, string Name, double Reliability, string Value);
}
