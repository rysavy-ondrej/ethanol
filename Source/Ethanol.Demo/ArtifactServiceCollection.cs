using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ethanol.Demo
{
    /// <summary>
    /// Collect services offering access to artifact data sources. It internally uses <see cref="ServiceCollection"/> class.
    /// <para/>
    /// Use <see cref="Build"/> to get service provider object for accessing the requested services.
    /// </summary>
    internal class ArtifactServiceCollection
    {
        IServiceCollection _services = new ServiceCollection();
        Dictionary<string, Type> _registeredArtifacts = new Dictionary<string, Type>();

        public ArtifactServiceCollection()
        {
            RegisterArtifactTypesFromAssembly(Assembly.GetExecutingAssembly());
        }
        public ArtifactServiceCollection(Assembly[] assemblies)
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


        public ArtifactServiceCollection AddArtifactProvider<TArtifact>(Func<IServiceProvider, IArtifactProvider<TArtifact>> implementationFactory) where TArtifact : IpfixArtifact 
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
            return new ArtifactServiceProvider(_services.BuildServiceProvider());
        }
    }
}