using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Plugins.Attributes;
using Ethanol.ContextBuilder.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ethanol.ContextBuilder.Plugins
{
    /// <summary>
    /// Abstract plugin factory.
    /// </summary>
    /// <typeparam name="TDestination">The object type of plugins to be created by the target factory.</typeparam>
    public abstract class PluginFactory<TDestination>
    {
        private readonly IDeserializer _configurationDeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
        Dictionary<string, (Type Type, PluginAttribute Plugin)> __factoryPlugins;

        protected PluginFactory()
        {
           var items  = Plugins.Value.Where(FilterPlugins).Select(p => new KeyValuePair<string, (Type Type, PluginAttribute Plugin)>(p.Plugin.Name,(p.Type, p.Plugin)));
            __factoryPlugins = new Dictionary<string, (Type Type, PluginAttribute Plugin)>(items);
        }

        /// <summary>
        /// Creates the plugin of the given name and configuration.
        /// </summary>
        /// <param name="name">The name of the plugin.</param>
        /// <param name="configString">The configuration string.</param>
        /// <returns>The created plugin object.</returns>
        public TDestination CreatePluginObject(string name, string configString)
        {
            if (__factoryPlugins.TryGetValue(name, out var readerType))
            {
                return (TDestination)CreateObject(readerType.Type, configString);
            }
            else
                return default;
        }

        /// <summary>
        /// Override to define a filter on plugins. 
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        protected abstract bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin);
        /// <summary>
        /// Creates a plugin object. 
        /// <para/>
        /// Each plugin should have Create method that has a single argument, which is a configuration object.
        /// </summary>
        /// <param name="pluginType">The type of plugin to create.</param>
        /// <param name="configurationString">The configuration string.</param>
        /// <returns></returns>
        protected object CreateObject(Type pluginType, string configurationString)
        {
            var createMethod = pluginType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(m => m.GetCustomAttributes(typeof(PluginCreateAttribute), false).Length > 0)
                                    .FirstOrDefault();
            if (createMethod == null) return null;
            var configurationType = createMethod.GetParameters().First().ParameterType;
            var parametersValue = _configurationDeserializer.Deserialize(configurationString, configurationType);
            var newObject = createMethod?.Invoke(null, new object[] { parametersValue });
            return newObject;
        }

        /// <summary>
        /// Gets the plugin of the given type. 
        /// </summary>
        /// <param name="pluginType">The type of plugins.</param>
        /// <returns>A dictionary of plugins of the given type.</returns>
        protected Dictionary<string, (Type Type, PluginAttribute Plugin)> GetPluginsOfType(PluginType pluginType)
        {
            return new Dictionary<string, (Type Type, PluginAttribute Plugin)>(
                Plugins.Value
                    .Where(p => p.Plugin.PluginType == pluginType)
                    .Select(x => new KeyValuePair<string, (Type Type, PluginAttribute Plugin)>(x.Plugin.Name, x)));
        }

        /// <summary>
        /// Lazy list of available plugins.
        /// </summary>
        static protected Lazy<List<(Type Type, PluginAttribute Plugin)>> Plugins = new Lazy<List<(Type Type, PluginAttribute Plugin)>>(CollectAvailablePLugins);

        /// <summary>
        /// Collects all available plugins in assemblies in the current domain. 
        /// </summary>
        /// <returns>A list of plugins and their types.</returns>
        static List<(Type Type, PluginAttribute Plugin)> CollectAvailablePLugins()
        {
            var plugins = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Select(t => (Type: t, Plugin: t.GetCustomAttribute<PluginAttribute>())).Where(t => t.Plugin != null));
            return plugins.ToList();
        }

        /// <summary>
        /// Gets plugin objects recognized by the factory.
        /// </summary>
        public IEnumerable<PluginAttribute> PluginObjects => __factoryPlugins.Values.Select(p => p.Plugin);
    }
}
