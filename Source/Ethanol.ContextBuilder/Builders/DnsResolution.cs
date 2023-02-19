using Ethanol.ContextBuilder.Context;
using System;
using System.Reflection.Metadata.Ecma335;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    ///  Represents selected information from DNS connection.
    /// </summary>
    public record DnsResolution
    {
        public DnsResolution(FlowKey flowKey, DnsRecordType queryType, DnsClass queryClass, DnsResponseCode responseCode, string questionName, string[] answerRecord, int responseTTL)
        {
            FlowKey = flowKey;
            QueryType = queryType;
            QueryClass = queryClass;
            ResponseCode = responseCode;
            QuestionName = questionName;
            AnswerRecord = answerRecord;
            ResponseTTL = responseTTL;
        }

        public FlowKey FlowKey { get; set; }
        public DnsRecordType QueryType { get; set; }
        public DnsClass QueryClass { get; set; }
        public DnsResponseCode ResponseCode { get; set; }
        public string QuestionName { get; set; }
        public string[] AnswerRecord { get; set; }
        public int ResponseTTL { get; set; }

        internal static DnsResolution Create(DnsFlow dnsFlow)
        {
            return new DnsResolution(dnsFlow.FlowKey, dnsFlow.QuestionType, dnsFlow.QuestionClass, dnsFlow.ResponseCode, dnsFlow.QuestionName, dnsFlow.ResponseData.Split(',') ?? Array.Empty<string>(), dnsFlow.ResponseTTL);
        }
    }
}
