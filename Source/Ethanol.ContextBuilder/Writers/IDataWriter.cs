using System;
using Ethanol.ContextBuilder.Pipeline;

namespace Ethanol.ContextBuilder.Writers
{
    public interface IDataWriter<TRecord> : IObserver<TRecord>, IPipelineNode
    {
    }
}
