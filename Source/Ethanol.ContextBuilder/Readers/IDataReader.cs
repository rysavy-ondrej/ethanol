using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Defines a contract for reader objects that can produce a sequence of records asynchronously.
    /// </summary>
    /// <typeparam name="TRecord">The type of records that the reader produces.</typeparam>
    public interface IDataReader<TRecord> : IObservable<TRecord>
    {
        /// <summary>
        /// Initiates the reading process asynchronously. Once started, the reader will produce records that can be observed.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ReadAllAsync(CancellationToken ct);
    }
}
