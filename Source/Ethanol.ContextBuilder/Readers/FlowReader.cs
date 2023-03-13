using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Ethanol.ContextBuilder.Pipeline;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// And interface for all reader objects.
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    public interface IFlowReader<TRecord> : IObservable<TRecord>, IPipelineNode
    {
        Task StartReading();
    }

    /// <summary>
    /// A base abstract class for input data readers.
    /// <para/>
    /// Input readers provide access to data sources. The type of data source is not limited 
    /// and can be file, standard input, database, or network conneciton. Readers 
    /// abstract from implementation details and provide data as observable sequence.
    /// </summary>
    /// <typeparam name="TRecord">The type of recrods to read.</typeparam>
    public abstract class FlowReader<TRecord> : IFlowReader<TRecord>
    {
        CancellationTokenSource _cts;
        Subject<TRecord> _subject;
        private Task _readingTask;

        public PipelineNodeType NodeType => PipelineNodeType.Producer;

        protected FlowReader()
        {
            _subject = new Subject<TRecord>();
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Opens the access to the data source.
        /// </summary>
        protected abstract void Open();

        /// <summary>
        /// Reads the next availabe record from the data source. 
        /// </summary>
        /// <param name="ct">Cancellation token to terminate the operation.</param>
        /// <param name="record">The record that was read.</param>
        /// <returns>true on success, false in case the record not was read.</returns>
        protected abstract bool TryGetNextRecord(CancellationToken ct, out TRecord record);

        /// <summary>
        /// Closes the data source access.
        /// </summary>
        protected abstract void Close();

        /// <summary>
        /// Starts a new task that readsdata from the data source. 
        /// <para/>
        /// Call this method after subscribing the observers to start processing the input data source.
        /// The returned task is awaitable.
        /// </summary>
        /// <returns>Task that completes when all data was processed.</returns>
        public Task StartReading()
        {
            _readingTask = Task.Factory.StartNew(ReaderProcedure, _cts.Token);
            return _readingTask;
        }

        /// <summary>
        /// A procedure that performs actual reading fromthe data source.
        /// </summary>
        private void ReaderProcedure()
        {
            var ct = _cts.Token;
            Open();
            while (!ct.IsCancellationRequested && TryGetNextRecord(ct, out var record))
            {
                _subject.OnNext(record);
            }
            _subject.OnCompleted();
            Close();
        }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<TRecord> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
