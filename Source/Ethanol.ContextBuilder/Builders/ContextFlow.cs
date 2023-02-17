using Ethanol.ContextBuilder.Classifiers;
using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Builders
{

    /// <summary>
    /// The context with flow object as the target entity.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="FlowKey"></param>
    /// <param name="Window"></param>
    /// <param name="Context"></param>
    public record FlowWithContext(string Id, FlowKey FlowKey, WindowSpan Window, TlsContext Context) : ContextObject<FlowKey, TlsContext>(Id, Window, FlowKey, Context);

    public record ClassifiedContextFlow<TContext>(FlowKey FlowKey, ClassificationResult[] Tags, TContext Context);
}
