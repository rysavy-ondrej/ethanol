namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents the different types of flows in a communication.
    /// </summary>
    public enum FlowType
    {
        /// <summary>
        /// Represents a flow initiating a request.
        /// </summary>
        RequestFlow,

        /// <summary>
        /// Represents a flow that is a response to a previous request.
        /// </summary>
        ResponseFlow,

        /// <summary>
        /// Represents a flow that is bidirectional, encompassing both request and response.
        /// </summary>
        BidirectionFlow,
        
        /// <summary>
        /// Represents a flow that is unidirectional, either request or response.
        /// </summary>
        UnidirectionalFlow
    }
}
