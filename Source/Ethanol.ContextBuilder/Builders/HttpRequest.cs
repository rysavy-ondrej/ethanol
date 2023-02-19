using Ethanol.ContextBuilder.Context;
using System;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    /// Represents selected information from HTTP connection. 
    /// </summary>
    public record HttpRequest
    {
        public HttpRequest(FlowKey flowKey, string url, string method, string response)
        {
            FlowKey = flowKey;
            Url = url;
            Method = method;
            Response = response;
        }

        public FlowKey FlowKey { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public string Response { get; set; }

        internal static HttpRequest Create(HttpFlow f)
        {
            return new HttpRequest(f.FlowKey, $"{f.Hostname}/{f.Url}", f.Method, f.ResultCode);
        }
    }
}
