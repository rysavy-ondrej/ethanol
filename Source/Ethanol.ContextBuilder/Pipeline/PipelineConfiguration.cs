using Ethanol.ContextBuilder.Enrichers;
using System;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Pipeline
{
    /// <summary>
    /// Represents the configuration settings for creating the processing pipeline.
    /// <para/>
    /// This configuration defines various parameters to shape the behavior and characteristics of the data processing pipeline.
    /// These settings are particularly utilized when invoking the <see cref="EthanolEnvironmentPipelines.CreateIpHostContextBuilderPipeline"/> method 
    /// to configure aspects like windowing, data enrichment, target host filtering, and more.
    /// </summary>
    public record PipelineConfiguration
    {
        /// <summary>
        /// Gets or sets the time interval for the analysis window. This defines the duration over which data is accumulated and analyzed in one go.
        /// </summary>
        [YamlMember(Alias = "window-size", Description = "The time interval of the analysis window.")]
        public TimeSpan WindowSize { get; set; }

        /// <summary>
        /// Gets or sets the time interval after which a new window is created. This determines how frequently new windows of data are initiated.
        /// </summary>
        [YamlMember(Alias = "window-hop", Description = "The hop interval of the window.")]
        public TimeSpan WindowHop { get; set; }

        /// <summary>
        /// Gets or sets the IP address prefix that defines the range of target context hosts. This allows for focusing on specific hosts or a range of hosts within a network.
        /// </summary>
        [YamlMember(Alias = "target-prefix", Description = "IP address prefix of target context host.")]
        public IPAddressPrefix TargetHostPrefix { get; set; }

        /// <summary>
        /// Gets or sets the delay interval between the builder and enricher in the data processing pipeline. 
        /// This delay can be essential for online processing scenarios, especially when awaiting data availability for the enricher.
        /// </summary>
        [YamlMember(Alias = "enricher-delay", Description = "Specifies the delay for data between builder and enricher. For online processing that includes flow tag enricher we need to wait until the data is available to enricher.")]
        public TimeSpan EnricherDelay { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets or sets the configuration settings for the uniform context enricher. This enricher ensures a consistent data context across the pipeline.
        /// </summary>
        [YamlMember(Alias = "tag-enricher", Description = "The configuration for the uniform context enricher.")]
        public EnricherConfiguration TagEnricherConfiguration { get; set; }
    }

}
