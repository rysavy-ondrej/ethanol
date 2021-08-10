using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ethanol.Demo
{
    public class ArtifactServiceProvider
    {
        private ServiceProvider _serviceProvider;

        public ArtifactServiceProvider(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
    }
}