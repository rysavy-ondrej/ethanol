using System;
using System.Threading.Tasks;
using Ethanol.ContextBuilder.Pipeline;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Defines a contract for reader objects that can produce a sequence of records asynchronously.
    /// </summary>
    /// <typeparam name="TRecord">The type of records that the reader produces.</typeparam>
    public interface IFlowReader<TRecord> : IObservable<TRecord>, IPipelineNode
    {
        /// <summary>
        /// Initiates the reading process asynchronously. Once started, the reader will produce records that can be observed.
        /// </summary>
        /// <returns>A task representing the asynchronous read operation.</returns>
        Task StartReading();
    }

}
