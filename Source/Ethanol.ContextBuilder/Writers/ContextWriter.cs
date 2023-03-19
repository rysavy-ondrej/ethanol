using System;
using System.Threading.Tasks;
using Ethanol.ContextBuilder.Pipeline;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Represets a base class for all writers. A writer outputs 
    /// context-based data in the specifc output format. 
    /// </summary>
    public abstract class ContextWriter<TRecord> : IObserver<TRecord>, IPipelineNode
    {
        bool _isopen = false;
        TaskCompletionSource _taskCompletionSource;
        protected ContextWriter()
        {
            _taskCompletionSource = new TaskCompletionSource();
        }

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
        /// Handles the receipt of a new value from the observed sequence by ensuring the writer is open, and then writing the value to it.
        /// This method is part of the IObserver interface implementation.
        /// </summary>
        /// <param name="value">The TRecord object representing the next value in the observed sequence.</param>
        public void OnNext(TRecord value)
        {
            if (!_isopen) { Open(); _isopen = true; }
            Write(value);
        }

        /// <summary>
        /// The task signalizes completion of the writing operation.
        /// </summary>
        public Task Completed => _taskCompletionSource.Task;

        public PipelineNodeType NodeType => PipelineNodeType.Sink;

        /// <summary>
        /// Override in the subclass to perform open operation on the underlying device.
        /// </summary>
        protected abstract void Open();

        /// <summary>
        /// Override in the subclass to perform write operation on the underlying device.
        /// </summary>
        protected abstract void Write(TRecord value);

        /// <summary>
        /// Override in the subclass to perform close operation on the underlying device.
        /// </summary>
        protected abstract void Close();
    }
}
