namespace Ethanol.ContextBuilder.Pipeline
{
    /// <summary>
    /// Represents a pipeline for processing ethanol data, which consists of a series of nodes that transform the data.
    /// </summary>
    public class EthanolPipeline
    {
        private readonly IPipelineNode[] nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="EthanolPipeline"/> class with the specified nodes.
        /// </summary>
        /// <param name="nodes">An array of <see cref="IPipelineNode"/> objects representing the nodes in the pipeline.</param>
        public EthanolPipeline(params IPipelineNode[] nodes)
        {
            this.nodes = nodes;
        }
    }
}
