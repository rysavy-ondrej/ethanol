using System;
using System.Linq;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="EthanolPipeline"/> class with a set of nodes that define the processing sequence.
        /// </summary>
        /// <param name="nodes">An array of <see cref="IPipelineNode"/> instances that constitute the stages or steps of the pipeline.</param>
        public EthanolPipeline(params IPipelineNode[] nodes)
        {
            this.nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        }

        /// <summary>
        /// Gets a task that represents the asynchronous completion of all nodes in the pipeline.
        /// </summary>
        public Task Completed => Task.WhenAll(nodes.Select(x => x.Completed).ToArray());

    }

}
