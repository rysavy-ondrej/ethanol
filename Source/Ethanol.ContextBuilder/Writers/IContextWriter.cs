using System;
using Ethanol.ContextBuilder.Pipeline;

namespace Ethanol.ContextBuilder.Writers
{
    public interface IContextWriter<TRecord> : IObserver<TRecord>, IPipelineNode
    {
    }
}
