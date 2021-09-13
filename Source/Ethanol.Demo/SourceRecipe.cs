using System;

namespace Ethanol.Demo
{
    public abstract record SourceRecipe(string ArtifactName, string FilterExpression)
    {
        public abstract Type ArtifactType { get; }

        public abstract void LoadFrom(string filename);
    }
    public record SourceRecipe<T>(string ArtifactName, string FilterExpression, ArtifactSource<T> ArtifactSource) : SourceRecipe(ArtifactName, FilterExpression) where T : IpfixArtifact
    {
        public override void LoadFrom(string filename)
        {
            ArtifactSource.LoadFrom(filename);
        }

        public override Type ArtifactType => typeof(T);
    }
}
