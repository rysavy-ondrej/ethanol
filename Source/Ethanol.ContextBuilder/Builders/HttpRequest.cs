using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Represents selected information from HTTP connection. 
    /// </summary>
    public record HttpRequest
    {
        public FlowKey FlowKey { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public string Response { get; set; }
    }
}
