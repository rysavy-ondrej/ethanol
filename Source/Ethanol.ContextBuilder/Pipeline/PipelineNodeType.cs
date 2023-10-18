namespace Ethanol.ContextBuilder.Pipeline
{
    /// <summary>
    /// Specifies the type of a pipeline node.
    /// </summary>
    public enum PipelineNodeType
    {
        /// <summary>
        /// Represents a producer node, responsible for generating and emitting data into the pipeline.
        /// </summary>
        Producer,

        /// <summary>
        /// Represents a transformer node, responsible for processing and transforming data as it flows through the pipeline.
        /// </summary>
        Transformer,

        /// <summary>
        /// Represents a filter node, responsible for selectively allowing data to pass through based on certain criteria.
        /// </summary>
        Filter,

        /// <summary>
        /// Represents a sink node, responsible for consuming and finalizing data from the pipeline.
        /// </summary>
        Sink
    }

}
