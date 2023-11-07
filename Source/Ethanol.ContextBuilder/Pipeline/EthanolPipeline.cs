using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Pipeline
{
    /// <summary>
    /// Represents a pipeline for sequentially processing data through a series of interconnected nodes.
    /// Each node within this pipeline performs specific transformations or operations on the data.
    /// </summary>
    public class EthanolPipeline
    {
        private readonly IPipelineNode[] nodes;
        private readonly Func<CancellationToken, Task[]> _startPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="EthanolPipeline"/> class with a set of nodes that define the processing sequence.
        /// </summary>
        /// <param name="nodes">An array of <see cref="IPipelineNode"/> instances that constitute the stages or steps of the pipeline.</param>
        public EthanolPipeline(IPipelineNode[] nodes, Func<CancellationToken, Task[]> start)
        {
            this.nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            this._startPipeline = start;
        }

        public async Task Start(CancellationToken ct)
        {
            // start the pipeline:
            var _ = _startPipeline(ct);
            await Task.WhenAll(nodes.Select(x => x.Completed).ToArray());
        }
    }

}
