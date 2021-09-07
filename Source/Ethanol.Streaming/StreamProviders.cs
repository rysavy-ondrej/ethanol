
using Ethanol.Artifacts;
using Ethanol.Providers;
using Microsoft.StreamProcessing;
namespace Ethanol.Streaming
{
    public static class StreamProviders
    {
        /// <summary>
        /// Gets the data as streamable collection to be processed by TRILL query.
        /// </summary>
        public static IStreamable<Empty, TArtifact> GetStreamable<TArtifact>(this IArtifactProvider<TArtifact> artifactProvider) where TArtifact : Artifact
        {
            return artifactProvider.GetObservable<TArtifact>().ToTemporalStreamable(x => x.StartTime, x => x.EndTime);
        }
    }
}
