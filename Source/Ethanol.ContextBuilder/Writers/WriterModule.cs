using Ethanol.ContextBuilder.Readers;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.Xml.Serialization;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Represets a base class for all writer modules. Writer modules produces 
    /// context-based information in the specifc output format. 
    /// </summary>
    public abstract class WriterModule<TRecord> : IObserver<TRecord>
    {
        bool _isopen = false;
        TaskCompletionSource _taskCompletionSource;
        protected WriterModule()
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
