using Microsoft.StreamProcessing.Aggregates;
using System.Linq.Expressions;

namespace Ethanol.Console
{
    public record FlowKey(string Proto, string SrcIp, int SrcPt, string DstIp, int DstPt)
    {
        public override string ToString() => $"{Proto}@{SrcIp}:{SrcPt}-{DstIp}:{DstPt}";
    }
    public record ContextFlow<TContext>(FlowKey FlowKey, TContext Context);
    public record ClassifiedContextFlow<TContext>(FlowKey FlowKey, ClassificationResult[] Tags, TContext Context);
}
