using Ethanol.Providers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ethanol.Artifacts
{
    /// <summary>
    /// Collect services offering access to artifact data sources. It internally uses <see cref="ServiceCollection"/> class.
    /// <para/>
    /// Use <see cref="Build"/> to get service provider object for accessing the requested services.
    /// </summary>
    public class ArtifactServiceCollection
    {
        IServiceCollection _services = new ServiceCollection();
        Dictionary<string, Type> _registeredArtifacts = new Dictionary<string, Type>();

        public ArtifactServiceCollection()
        {
            RegisterArtifactTypesFromAssembly(Assembly.GetExecutingAssembly());
        }
        public ArtifactServiceCollection(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterArtifactTypesFromAssembly(assembly);
            }
        }

        void RegisterArtifactTypesFromAssembly(Assembly assembly)
        {
            (Type ArtifactType, ArtifactNameAttribute ArtifactName) getArtifactName(Type type)
            {
                var attName = type.GetCustomAttribute<ArtifactNameAttribute>();
                return (type, attName);
            }
            foreach (var artifactType in assembly.GetTypes().Select(getArtifactName).Where(t => t.ArtifactName != null))
            {
                _registeredArtifacts.Add(artifactType.ArtifactName.Name, artifactType.ArtifactType);
            }
        }

        public Type GetArtifactTypeByName(string artifactName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return _registeredArtifacts.FirstOrDefault(x => string.Equals(x.Key, artifactName, stringComparison)).Value;
        }


        public ArtifactServiceCollection AddArtifactProvider<TArtifact>(Func<IServiceProvider, IArtifactProvider<TArtifact>> implementationFactory) where TArtifact : Artifact 
        {
            _services.AddTransient(implementationFactory);
            return this;
        }


        internal ArtifactServiceCollection AddArtifactProvider(Type artifactType, Func<IServiceProvider, object> implementationFactory)
        {
            var targetType = typeof(IArtifactProvider<>).MakeGenericType(artifactType);
            _services.AddTransient(targetType, implementationFactory);
            return this;
        }

        public ArtifactServiceProvider Build()
        {
            return new ArtifactServiceProvider(_services.BuildServiceProvider(), _services.Select(s=>s.ServiceType.GenericTypeArguments.First()));
        }
    }

    public static class ArtifactServiceCollectionExtensions
    {
        /// <summary>
        /// Add artifact providers based on the CSV file in the specified directory.
        /// </summary>
        /// <param name="path">The path to the folder with data files.</param>
        /// <returns></returns>
        public static void AddArtifactFromCsvFiles(this ArtifactServiceCollection artifactServiceCollection, string path)
        {
            foreach (var file in Directory.GetFiles(path, "*.csv"))
            {
                var artifactType = artifactServiceCollection.GetArtifactTypeByName(Path.GetFileNameWithoutExtension(file));
                if (artifactType != null)
                {
                    artifactServiceCollection.AddArtifactProvider(artifactType, s => CsvArtifactProvider.CreateArtifactSource(artifactType, file));
                }
            }
        }
    }
}