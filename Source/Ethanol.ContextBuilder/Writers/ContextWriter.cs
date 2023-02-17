using System;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Represets a base class for all writers. A writer outputs 
    /// context-based data in the specifc output format. 
    /// </summary>
    public abstract class ContextWriter<TRecord> : IObserver<TRecord>
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


        /// <inheritdoc/>
        public void OnNext(TRecord value)
        {
            if (!_isopen) { Open(); _isopen = false; }
            Write(value);
        }

        /// <summary>
        /// The task signalizes completion of the writing operation.
        /// </summary>
        public Task Completed => _taskCompletionSource.Task;

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
