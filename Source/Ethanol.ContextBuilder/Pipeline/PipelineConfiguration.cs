using Ethanol.ContextBuilder.Enrichers;
using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Pipeline
{
    /// <summary>
    /// Represents the configuration settings for creating the processing pipeline.
    /// <para/>
    /// Used, for instance, with <see cref="EthanolEnvironmentPipelines.CreateIpHostContextBuilderPipeline"/> method.
    /// </summary>
    public record PipelineConfiguration
    {
        [YamlMember(Alias = "window-size", Description = "The time interval of the analysis window.")]
        public TimeSpan WindowSize { get; set; }

        [YamlMember(Alias = "window-hop", Description = "The hop interval of the window. ")]
        public TimeSpan WindowHop { get; set; }

        [YamlMember(Alias = "enricher-delay", Description = "Specifies the delay for data between builder and enricher. For online processing that includes flow tag enricher we need to wait until the data is available to enricher.")]
        public TimeSpan EnricherDelay { get; set; } = TimeSpan.Zero;

        [YamlMember(Alias = "host-tag-enricher", Description = "The configuration for the IP host context enricher.")]
        public IpHostContextEnricherPlugin.DataSourceEnricherConfiguration HostTagEnricherConfiguration { get; set; }

        [YamlMember(Alias = "flow-tag-enricher", Description = "The configuration for the flow tag enricher.")]
        public IpHostContextEnricherPlugin.DataSourceEnricherConfiguration FlowTagEnricherConfiguration { get; set; }
    }


    /// <summary>
    /// Provides methods for serializing and deserializing pipeline configurations to and from YAML format.
    /// </summary>
    public static class PipelineConfigurationSerializer
    {
        static YamlDotNet.Serialization.Deserializer deserializer = new YamlDotNet.Serialization.Deserializer();
        static YamlDotNet.Serialization.Serializer serializer = new YamlDotNet.Serialization.Serializer();

        /// <summary>
        /// Creates a new <see cref="PipelineConfiguration"/> object from the specified YAML string.
        /// </summary>
        /// <param name="text">The YAML string to deserialize.</param>
        /// <returns>A new <see cref="PipelineConfiguration"/> object representing the deserialized YAML data.</returns>
        public static PipelineConfiguration CreateFrom(string text)
        {
            var config = deserializer.Deserialize<PipelineConfiguration>(text);
            return config;
        }

        /// <summary>
        /// Loads a <see cref="PipelineConfiguration"/> object from the specified YAML file.
        /// </summary>
        /// <param name="path">The path to the YAML file to load.</param>
        /// <returns>A new <see cref="PipelineConfiguration"/> object representing the deserialized YAML data.</returns>
        public static PipelineConfiguration LoadFromFile(string path)
        {
            var configurationString = System.IO.File.ReadAllText(path);
            var config = deserializer.Deserialize<PipelineConfiguration>(configurationString);
            return config;
        }

        /// <summary>
        /// Serializes the specified <see cref="PipelineConfiguration"/> object to YAML format and saves it to the specified file.
        /// </summary>
        /// <param name="configuration">The <see cref="PipelineConfiguration"/> object to serialize.</param>
        /// <param name="path">The path to the file to save the YAML data to.</param>
        public static void SaveToFile(PipelineConfiguration configuration, string path)
        {
            var configurationString = serializer.Serialize(configuration);
            File.AppendAllText(path, configurationString);
        }
    }

}
