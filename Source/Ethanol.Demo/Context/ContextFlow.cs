using Microsoft.StreamProcessing.Aggregates;
using System.Linq.Expressions;

namespace Ethanol.Demo
{
    public record Flow(string Proto, string SrcIp, int SrcPt, string DstIp, int DstPt);
    public record ContextFlow<TContext>(Flow Flow, TContext Context);
    public record ClassifiedContextFlow<TContext>(Flow Flow, ClassificationResult[] Tags, TContext Context);
}
