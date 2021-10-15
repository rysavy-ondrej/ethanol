using Microsoft.StreamProcessing.Aggregates;
using System.Linq.Expressions;

namespace Ethanol.Demo
{
    public record FlowKey(string Proto, string SrcIp, int SrcPt, string DstIp, int DstPt)
    {
        public override string ToString() => $"{Proto}@{SrcIp}:{SrcPt}-{DstIp}:{DstPt}";
    }
    public record ContextFlow<TContext>(FlowKey Flow, TContext Context);
    public record ClassifiedContextFlow<TContext>(FlowKey Flow, ClassificationResult[] Tags, TContext Context);
}
