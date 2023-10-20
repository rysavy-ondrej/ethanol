using System;
using System.Threading.Tasks;
using Ethanol.ContextBuilder.Pipeline;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Represents a base class for all context writers. A writer outputs 
    /// context-based data in a specific output format. 
    /// </summary>
    /// <typeparam name="TRecord">The type of record the context writer will handle.</typeparam>
    public abstract class ContextWriter<TRecord> : IObserver<TRecord>, IPipelineNode
    {
        bool _isopen = false;
        TaskCompletionSource _taskCompletionSource = new TaskCompletionSource();

        /// <inheritdoc/>
        public void OnCompleted()
        {
            Close();
            _taskCompletionSource.SetResult();
        }


        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            _taskCompletionSource.SetException(error);
        }

        /// <summary>
        /// Handles the receipt of a new value from the observed sequence by ensuring the writer is open, 
        /// and then writing the value to it. This method is part of the IObserver interface implementation.
        /// </summary>
        /// <param name="value">The TRecord object representing the next value in the observed sequence.</param>
        public void OnNext(TRecord value)
        {
            if (!_isopen) { Open(); _isopen = true; }
            Write(value);
        }

        /// <summary>
        /// Gets a task that signals the completion of the writing operation.
        /// </summary>
        public Task Completed => _taskCompletionSource.Task;

        /// <summary>
        /// Gets the type of the pipeline node, indicating it's a sink (end of the pipeline).
        /// </summary>
        public PipelineNodeType NodeType => PipelineNodeType.Sink;

        /// <summary>
        /// Opens the underlying device or stream for writing. This method should be overridden in derived classes 
        /// to provide specific open logic for the writer.
        /// </summary>
        protected abstract void Open();

        /// <summary>
        /// Writes the given record to the underlying device or stream. This method should be overridden in derived classes 
        /// to provide specific write logic for the writer.
        /// </summary>
        /// <param name="value">The record to write.</param>
        protected abstract void Write(TRecord value);

        /// <summary>
        /// Closes the underlying device or stream, finalizing the write operations. This method should be overridden in derived classes 
        /// to provide specific close logic for the writer.
        /// </summary>
        protected abstract void Close();
    }
}
