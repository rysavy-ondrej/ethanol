using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ethanol.Artifacts
{
    public static class ArtifactFactory
    {
        public static Dictionary<string, Type> _registeredArtifacts = new Dictionary<string, Type>();

        public static void LoadArtifactsFromAssembly(Assembly assembly)
        {
            (Type ArtifactType, ArtifactNameAttribute ArtifactName) getArtifactName(Type type)
            {
                var attName = type.GetCustomAttribute<ArtifactNameAttribute>();
                return (type,attName);
            }
            foreach(var artifactType in assembly.GetTypes().Select(getArtifactName).Where(t => t.ArtifactName != null))
            {
                _registeredArtifacts.Add(artifactType.ArtifactName.Name, artifactType.ArtifactType);
            }
        }

        public static Type GetArtifact(string artifactName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return _registeredArtifacts.FirstOrDefault(x => string.Equals(x.Key, artifactName, stringComparison)).Value;
        }
    }
}