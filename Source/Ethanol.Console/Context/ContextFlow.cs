using Microsoft.StreamProcessing.Aggregates;
using System.Linq.Expressions;

namespace Ethanol.Console
{
    public record ContextFlow<TContext>(FlowKey FlowKey, TContext Context);
    public record ClassifiedContextFlow<TContext>(FlowKey FlowKey, ClassificationResult[] Tags, TContext Context);
}
