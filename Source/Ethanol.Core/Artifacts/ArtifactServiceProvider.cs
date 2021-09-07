using Ethanol.Providers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ethanol.Artifacts
{
    public class ArtifactServiceProvider
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Type[] _serviceTypes;
        public ArtifactServiceProvider(ServiceProvider serviceProvider, IEnumerable<Type> serviceTypes)
        {
            _serviceProvider = serviceProvider;
            _serviceTypes = serviceTypes.ToArray(); ;
        }

        public IArtifactProvider GetService(Type artifactType)
        {
            var targetType = typeof(IArtifactProvider<>).MakeGenericType(artifactType);
            return _serviceProvider.GetService(targetType) as IArtifactProvider;
        }
        public IArtifactProvider<TArtifact> GetService<TArtifact>() where TArtifact : Artifact
        {            
            return _serviceProvider.GetService<IArtifactProvider<TArtifact>>();
        }

        public IEnumerable<Type> Services => _serviceTypes;
    }
}