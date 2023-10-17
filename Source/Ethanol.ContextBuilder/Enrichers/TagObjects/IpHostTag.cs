using System;

namespace Ethanol.ContextBuilder.Enrichers.TagObjects
{
    /// <summary>
    /// Represents information about a host during a specified time interval.
    /// </summary>
    public record IpHostTag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IpHostTag"/> record with the specified parameters.
        /// </summary>
        /// <param name="startTime">The start time of the host's activity.</param>
        /// <param name="endTime">The end time of the host's activity.</param>
        /// <param name="hostAddress">The IP address of the host.</param>
        /// <param name="name">The name or identifier of the host.</param>
        /// <param name="reliability">A measure of the reliability of the data (usually between 0.0 and 1.0).</param>
        /// <param name="value">Additional value or information about the host.</param>
        public IpHostTag(DateTime startTime, DateTime endTime, string hostAddress, string name, double reliability, string value)
        {
            StartTime = startTime;
            EndTime = endTime;
            HostAddress = hostAddress;
            Name = name;
            Reliability = reliability;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the start time of the host's activity.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the host's activity.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the host.
        /// </summary>
        public string HostAddress { get; set; }

        /// <summary>
        /// Gets or sets the name or identifier of the host.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a measure of the reliability of the data (usually between 0.0 and 1.0).
        /// </summary>
        public double Reliability { get; set; }

        /// <summary>
        /// Gets or sets additional value or information about the host.
        /// </summary>
        public string Value { get; set; }
    }
}
