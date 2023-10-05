using System;

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
    public record HostTag
    {
        public HostTag(DateTime startTime, DateTime endTime, string hostAddress, string name, double reliability, string value)
        {
            StartTime = startTime;
            EndTime = endTime;
            HostAddress = hostAddress;
            Name = name;
            Reliability = reliability;
            Value = value;
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string HostAddress { get; set; }
        public string Name { get; set; }

        public double Reliability { get; set; }
        public string Value { get; set; }
    }

}
