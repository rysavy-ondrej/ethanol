using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Ethanol.ContextBuilder.Pipeline;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// A base abstract class for input data readers. This abstract class simplifies the development of specific flow readers by abstracting out common logic related to data streaming, 
    /// task management, and handling of cancellation tokens.
    /// <para/>
    /// Input readers provide access to data sources. The type of data source is not limited 
    /// and can be file, standard input, database, or network conneciton. Readers 
    /// abstract from implementation details and provide data as observable sequence.
    /// </summary>
    /// <typeparam name="TRecord">The type of recrods to read.</typeparam>
    public abstract class BaseFlowReader<TRecord> : IFlowReader<TRecord>
    {
        private Subject<TRecord> _subject = new Subject<TRecord>();
        private Task _readingTask = Task.CompletedTask;

        /// <summary>
        /// Gets the node type of this reader, which is always a producer in the pipeline.
        /// </summary>
        public PipelineNodeType NodeType => PipelineNodeType.Producer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFlowReader{TRecord}"/> class.
        /// </summary>
        protected BaseFlowReader()
        {
        }

        /// <summary>
        /// Prepares and initializes the data source for reading. This could involve actions like
        /// establishing a connection to a database, opening a file stream, or setting up network connections.
        /// </summary>
        /// <remarks>
        /// Implementations should ensure that after this method is called, the data source is ready for reading.
        /// This method is called once at the beginning of the reading process.
        /// </remarks>
        protected abstract void Open();

        /// <summary>
        /// Attempts to read the next record from the data source.
        /// </summary>
        /// <param name="ct">A token to observe while waiting for the task to complete, to cancel the read operation if necessary.</param>
        /// <param name="record">When this method returns, contains the next record from the data source if available, or the default value of <typeparamref name="TRecord"/> if not.</param>
        /// <returns>true if a record was successfully read; otherwise, false.</returns>
        /// <remarks>
        /// Implementations should ensure that this method is efficient and can be called repeatedly to fetch records in quick succession.
        /// </remarks>
        protected abstract bool TryGetNextRecord(CancellationToken ct, out TRecord record);

        /// <summary>
        /// Cleans up resources and finalizes access to the data source. This could involve actions like
        /// closing a database connection, releasing a file stream, or shutting down network connections.
        /// </summary>
        /// <remarks>
        /// This method is called once after all reading operations are complete, even if an error occurs during reading.
        /// </remarks>
        protected abstract void Close();

        /// <summary>
        /// Initiates the asynchronous reading process from the data source. This method starts a new task 
        /// that will continuously read records from the data source until no more records are available or 
        /// the operation is cancelled.
        /// </summary>
        /// <returns>A task representing the asynchronous reading operation.</returns>
        /// <remarks>
        /// Callers can await this task to be notified when the entire reading process is complete.
        /// </remarks>
        public void StartReading(CancellationToken ct)
        {
            _readingTask = Task.Factory.StartNew(() =>
            {
                Open();
                while (!ct.IsCancellationRequested && TryGetNextRecord(ct, out var record))
                {
                    _subject.OnNext(record);
                }
                _subject.OnCompleted();
                Close();
            }
            , ct);
        }
        /// <summary>
        /// Gets a task that represents the completion of the reading process. Awaiting this task will 
        /// wait until all reading operations are complete.
        /// </summary>
        public Task Completed => _readingTask;

        /// <summary>
        /// Subscribes an observer to the subject that notifies them with new records as they are read.
        /// </summary>
        /// <param name="observer">The observer that wants to receive notifications of new records.</param>
        /// <returns>An IDisposable that, when disposed, will stop sending notifications to the observer.</returns>
        public IDisposable Subscribe(IObserver<TRecord> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
