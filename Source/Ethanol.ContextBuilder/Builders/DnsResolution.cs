using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    ///  Represents selected information from DNS connection.
    /// </summary>
    public record DnsResolution
    {
        public FlowKey FlowKey { get; set; }
        public DnsRecordType QueryType { get; set; }
        public DnsClass QueryClass { get; set; }
        public DnsResponseCode ResponseCode { get; set; }
        public string QuestionName { get; set; }
        public string[] AnswerRecord { get; set; }
        public int ResponseTTL { get; set; }
    }
}
