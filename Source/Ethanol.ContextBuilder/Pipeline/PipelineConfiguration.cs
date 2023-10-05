using Ethanol.ContextBuilder.Enrichers;
using System;
using System.IO;
using System.Text.RegularExpressions;
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

        [YamlMember(Alias = "target-prefix", Description = "IP address prefix of target context host.")]
        public IPAddressPrefix TargetHostPrefix { get; set; }

        [YamlMember(Alias = "enricher-delay", Description = "Specifies the delay for data between builder and enricher. For online processing that includes flow tag enricher we need to wait until the data is available to enricher.")]
        public TimeSpan EnricherDelay { get; set; } = TimeSpan.Zero;

        [YamlMember(Alias = "tag-enricher", Description = "The configuration for the uniform context enricher.")]
        public IpHostContextEnricherPlugin.DataSourceEnricherConfiguration TagEnricherConfiguration { get; set; }        
    }


    /// <summary>
    /// Provides methods for serializing and deserializing pipeline configurations to and from YAML format.
    /// </summary>
    public static class PipelineConfigurationSerializer
    {
        static NLog.Logger __logger = NLog.LogManager.GetCurrentClassLogger();
        static IDeserializer deserializer = new DeserializerBuilder().WithTypeConverter(new IPAddressPrefixTypeConverter()).Build();
        static ISerializer serializer = new SerializerBuilder().WithTypeConverter(new IPAddressPrefixTypeConverter()).Build();

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
        public static PipelineConfiguration LoadFromFile(string path, bool resolveEnvironmentVariables = true)
        {
            var configurationString = System.IO.File.ReadAllText(path);
            if (resolveEnvironmentVariables)
            {
                configurationString = ResolveEnvironmentVariables(configurationString);
            }
            __logger.Info($"Loading pipeline configuration: {configurationString}");
            var config = deserializer.Deserialize<PipelineConfiguration>(configurationString);
            return config;
        }

        /// <summary>
        /// Replaces every reference to an environment variables, e.g., ${VARIABLE}, by its content or empty string if the variable is not set.
        /// </summary>
        private static string ResolveEnvironmentVariables(string input)
        {
            Regex regex = new Regex(@"\$\{([\w_]+)\}");
            string output = regex.Replace(input, match =>
            {
                string variableName = match.Groups[1].Value;
                string variableValue = Environment.GetEnvironmentVariable(variableName);
                return variableValue ?? String.Empty;
            });
            
            return output;
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
