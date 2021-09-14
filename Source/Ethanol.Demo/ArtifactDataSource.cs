using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    /// <summary>
    /// An abstract base class for artifact sources. It provides a recipe for
    /// accessing IPFIX records by specifying the artifact type and expression. 
    /// </summary>
    public abstract record ArtifactDataSource(string ArtifactName, string FilterExpression)
    {
        /// <summary>
        /// Gets the artifact type of the current recipe.
        /// </summary>
        public abstract Type ArtifactType { get; }

        /// <summary>
        /// Loads records from the given file to the associated <see cref="ArtifactSourceObservable{T}"/>.
        /// </summary>
        /// <param name="filename">The source file to load IPFIX records from.</param>
        /// <param name="cancellationToken">the cancellation token to stop the operation.</param>
        /// <returns>The task that completes when all records from the given file were fetched.</returns>
        public abstract Task<int> LoadFromAsync(Stream stream, CancellationToken cancellationToken);

        public abstract void Close();
    }
    /// <summary>
    ///  
    /// </summary>
    /// <typeparam name="T">The type of artifacts.</typeparam>
    public sealed record ArtifactDataSource<T>(string ArtifactName, string FilterExpression, ArtifactSourceObservable<T> ArtifactSource) : ArtifactDataSource(ArtifactName, FilterExpression) where T : IpfixArtifact
    {
        public override Task<int> LoadFromAsync(Stream stream, CancellationToken cancellationToken)
        {
            return Task.Run(() => ArtifactSource.LoadFrom(stream, cancellationToken));
        }

        public override void Close()
        {
            ArtifactSource.Close();
        }

        public override Type ArtifactType => typeof(T);
    }
}
