using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Pipeline
{

    /// <summary>
    /// Defines a contract for a node within the ethanol processing pipeline. A node can assume various roles, such as producing, transforming, filtering, or sinking data, to facilitate the data processing flow.
    /// </summary>
    public interface IPipelineNode
    {
        /// <summary>
        /// Specifies the distinct role or function the node plays within the ethanol processing pipeline.
        /// </summary>
        PipelineNodeType NodeType { get; }

        /// <summary>
        /// Represents a task that signals the completion of the node's operation or processing.
        /// </summary>
        Task Completed { get; }
    }

}
