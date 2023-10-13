namespace Ethanol.ContextBuilder.Pipeline
{
    /// <summary>
    /// Represents a node in an ethanol processing pipeline, which can either produce, transform, filter, or sink data.
    /// </summary>
    public interface IPipelineNode
    {
        /// <summary>
        /// Gets the type of the pipeline node.
        /// </summary>
        PipelineNodeType NodeType { get; }
    }
}
