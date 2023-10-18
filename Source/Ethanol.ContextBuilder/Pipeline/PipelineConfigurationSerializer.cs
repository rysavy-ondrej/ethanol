using Ethanol.ContextBuilder.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace Ethanol.ContextBuilder.Pipeline
{
    /// <summary>
    /// Facilitates the serialization and deserialization of pipeline configurations, enabling conversions between object representations and YAML formats.
    /// </summary>
    public static class PipelineConfigurationSerializer
    {
        static ILogger __logger = LogManager.GetCurrentClassLogger();
        static IDeserializer deserializer = new DeserializerBuilder().WithTypeConverter(new IPAddressPrefixYamlTypeConverter()).Build();
        static ISerializer serializer = new SerializerBuilder().WithTypeConverter(new IPAddressPrefixYamlTypeConverter()).Build();

        /// <summary>
        /// Transforms a YAML string representation into its corresponding <see cref="PipelineConfiguration"/> object.
        /// </summary>
        /// <param name="text">The YAML formatted string to be deserialized.</param>
        /// <returns>An instance of <see cref="PipelineConfiguration"/> populated with the data from the YAML string.</returns>
        public static PipelineConfiguration CreateFrom(string text)
        {
            var config = deserializer.Deserialize<PipelineConfiguration>(text);
            return config;
        }

        /// <summary>
        /// Deserializes a <see cref="PipelineConfiguration"/> object from a given YAML file. This method also supports resolving environment variables referenced within the YAML.
        /// </summary>
        /// <param name="path">The path to the YAML file.</param>
        /// <param name="resolveEnvironmentVariables">Indicates whether environment variable references in the YAML, such as ${VARIABLE}, should be resolved. Default is true.</param>
        /// <returns>An instance of <see cref="PipelineConfiguration"/> representing the deserialized data from the YAML file.</returns>
        public static PipelineConfiguration LoadFromFile(string path, bool resolveEnvironmentVariables = true)
        {
            var configurationString = System.IO.File.ReadAllText(path);
            if (resolveEnvironmentVariables)
            {
                configurationString = ResolveEnvironmentVariables(configurationString);
            }
            __logger.LogInformation($"Loading pipeline configuration: {configurationString}");
            var config = deserializer.Deserialize<PipelineConfiguration>(configurationString);
            return config;
        }

        /// <summary>
        /// Converts environment variable references in the input string to their actual values. If an environment variable is not set, its reference is replaced with an empty string.
        /// </summary>
        /// <param name="input">The input string potentially containing environment variable references.</param>
        /// <returns>The input string with environment variable references replaced with their actual values.</returns>
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
        /// Converts the given <see cref="PipelineConfiguration"/> object to its YAML representation and writes it to a specified file.
        /// </summary>
        /// <param name="configuration">The <see cref="PipelineConfiguration"/> object to be serialized.</param>
        /// <param name="path">The file path where the serialized YAML data should be saved.</param>
        public static void SaveToFile(PipelineConfiguration configuration, string path)
        {
            var configurationString = serializer.Serialize(configuration);
            File.AppendAllText(path, configurationString);
        }
    }
}
