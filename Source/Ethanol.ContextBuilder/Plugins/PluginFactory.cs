using Ethanol.ContextBuilder.Plugins.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ethanol.ContextBuilder.Plugins
{
    /// <summary>
    /// Serves as a base factory class to facilitate the instantiation of plugins. It abstracts the common functionalities and patterns 
    /// involved in creating plugin objects and delegates specific plugin filtering logic to derived classes.
    /// </summary>
    /// <typeparam name="TDestination">
    /// The type of plugin objects the factory is responsible for creating. This ensures that the created plugin objects adhere to the
    /// desired interface or base class.
    /// </typeparam>
    public abstract class PluginFactory<TDestination>
    {
        ILogger __logger =null;

        /// <summary>
        /// Helps in deserializing the configuration for plugins, using YAML conventions with camel casing.
        /// </summary>
        private readonly IDeserializer _configurationDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        /// <summary>
        /// A collection that maps plugin names to their corresponding types and metadata attributes.
        /// </summary>
        private Dictionary<string, (Type Type, PluginAttribute Plugin)> __factoryPlugins;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginFactory{TDestination}"/> class. This constructor populates 
        /// the internal mapping of plugins based on the provided filtering criteria.
        /// </summary>
        protected PluginFactory()
        {
            var items = Plugins.Value
                .Where(FilterPlugins)
                .Select(p => new KeyValuePair<string, (Type Type, PluginAttribute Plugin)>(p.Plugin.Name, (p.Type, p.Plugin)));
            __factoryPlugins = new Dictionary<string, (Type Type, PluginAttribute Plugin)>(items);
        }

        /// <summary>
        /// Instantiates a plugin object based on its name and configuration string. This method leverages the internal plugin 
        /// mapping to identify the correct type and subsequently creates an instance of the desired plugin.
        /// </summary>
        /// <param name="name">The name of the plugin to be created. This name should correspond to a plugin registered within the factory.</param>
        /// <param name="configString">The configuration string which specifies how the plugin should be set up. This is typically in a format that the deserializer can interpret.</param>
        /// <returns>
        /// An instance of the plugin type represented by <typeparamref name="TDestination"/>. Returns the default value (typically null) if the plugin name is not recognized.
        /// </returns>
        public TDestination CreatePluginObject(string name, string configString)
        {
            if (__factoryPlugins.TryGetValue(name, out var readerType))
            {
                return (TDestination)CreateObject(readerType.Type, configString);
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Defines the criteria based on which plugins are selected for inclusion in the factory's internal mapping. Derived classes 
        /// are expected to provide the specific logic to filter out unwanted plugins.
        /// </summary>
        /// <param name="plugin">
        /// A tuple containing the type of the plugin and its associated <see cref="PluginAttribute"/> metadata.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the provided plugin should be included (true) or excluded (false) in the factory's internal mapping.
        /// </returns>
        protected abstract bool FilterPlugins((Type Type, PluginAttribute Plugin) plugin);


        /// <summary>
        /// Instantiates a plugin object of the specified type. The plugin's class should contain a static method 
        /// annotated with the <see cref="PluginCreateAttribute"/>, which is used to create the plugin instance.
        /// </summary>
        /// <param name="pluginType">The type of the plugin to instantiate.</param>
        /// <param name="configurationString">A string containing configuration data for the plugin. This data is passed to the plugin's creation method.</param>
        /// <returns>An instance of the specified plugin type, configured according to the provided configuration string.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified plugin type lacks an appropriate creation method annotated with <see cref="PluginCreateAttribute"/>.</exception>
        protected object CreateObject(Type pluginType, string configurationString)
        {
            var createMethod = pluginType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(m => m.GetCustomAttributes(typeof(PluginCreateAttribute), false).Length > 0)
                                    .FirstOrDefault();
            if (createMethod == null) throw new InvalidOperationException($"The {pluginType} has not Create method annotated with [PluginCreate].");
            var configurationType = createMethod.GetParameters().First().ParameterType;
            var parametersValue = GetParameterValues(configurationString, configurationType);
            var newObject = createMethod?.Invoke(null, new object[] { parametersValue });
            return newObject;
        }

        /// <summary>
        /// Converts the provided configuration string into a configuration object of the specified type.
        /// If the provided string is empty or null, a default instance of the specified type is created and returned.
        /// </summary>
        /// <param name="configString">A string containing configuration data.</param>
        /// <param name="configType">The type of configuration object to create.</param>
        /// <returns>An instance of the specified configuration type, populated with data from the provided configuration string.</returns>
        /// <exception cref="Exception">Thrown if there is an error deserializing the configuration string into the desired type.</exception>
        private object GetParameterValues(string configString, Type configType)
        {
            if (String.IsNullOrWhiteSpace(configString))
            {
                return configType.GetConstructor(Array.Empty<Type>()).Invoke(null);
            }
            else
            {
                try
                { 
                    return _configurationDeserializer.Deserialize(configString, configType);
                }
                catch (Exception e)
                {
                    throw new Exception($"ERROR: Cannot parse input arguments {configString}: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Retrieves a collection of available plugins that belong to the specified category.
        /// </summary>
        /// <param name="pluginType">The category of plugins to retrieve.</param>
        /// <returns>A dictionary mapping plugin names to their respective types and attributes, filtered by the specified category.</returns>
        protected Dictionary<string, (Type Type, PluginAttribute Plugin)> GetPluginsOfType(PluginCategory pluginType)
        {
            return new Dictionary<string, (Type Type, PluginAttribute Plugin)>(
                Plugins.Value
                    .Where(p => p.Plugin.Category == pluginType)
                    .Select(x => new KeyValuePair<string, (Type Type, PluginAttribute Plugin)>(x.Plugin.Name, x)));
        }

        /// <summary>
        /// Lazy list of available plugins.
        /// </summary>
        static protected Lazy<List<(Type Type, PluginAttribute Plugin)>> Plugins = new Lazy<List<(Type Type, PluginAttribute Plugin)>>(CollectAvailablePLugins);

        /// <summary>
        /// Contains a lazily-initialized list of all available plugins. The list is populated using the <see cref="CollectAvailablePLugins"/> method, which scans the assembly for classes annotated with the <see cref="PluginAttribute"/>.
        /// </summary>
        static List<(Type Type, PluginAttribute Plugin)> CollectAvailablePLugins()
        {
            var plugins = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Select(t => (Type: t, Plugin: t.GetCustomAttribute<PluginAttribute>(false))).Where(t => t.Plugin != null)).ToList(); ;
            /*
            foreach (var p in plugins)
            {
                __logger?.LogDebug($"Available plugin: name={p.Plugin.Name}, category={p.Plugin.Category}, class={p.Type}.");
            }*/
            return plugins;
        }

        /// <summary>
        /// Retrieves the collection of plugin attributes associated with the plugins that are recognized and managed by this factory.
        /// This provides a way to inspect the meta-information about the plugins, such as their names, descriptions, and categories, 
        /// without instantiating the actual plugin objects.
        /// </summary>
        /// <remarks>
        /// This property essentially offers a view into the factory's internal registry of plugins. 
        /// It's useful for scenarios where one needs to list or inspect available plugins without necessarily 
        /// creating instances of them.
        /// </remarks>
        public IEnumerable<PluginAttribute> PluginObjects => __factoryPlugins.Values.Select(p => p.Plugin);

    }
}
