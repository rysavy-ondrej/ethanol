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
    public abstract class OutputDataWriter<TRecord> : IObserver<TRecord>
    {
        bool _isopen = false;
        TaskCompletionSource _taskCompletionSource;
        protected OutputDataWriter()
        {
            _taskCompletionSource = new TaskCompletionSource();           
        }

        public void OnCompleted()
        {
            Close();
            _taskCompletionSource.SetResult();
        }

        public void OnError(Exception error)
        {
            _taskCompletionSource.SetException(error);
        }
        

        public void OnNext(TRecord value)
        {
            if (!_isopen) { Open(); _isopen = false; }
            Write(value);
        }

        /// <summary>
        /// The task that signalizes completion of the writing.
        /// </summary>
        public Task Completed => _taskCompletionSource.Task;

        protected abstract void Open();

        protected abstract void Write(TRecord value);

        protected abstract void Close();
    }

    public static class WriterFactory
    {
        internal static OutputDataWriter<object> GetWriter(ModuleSpecification outputModule)
        {
            switch(outputModule?.Name)
            {
                case nameof(YamlDataWriter): return YamlDataWriter.Create(outputModule.Attributes);
                case nameof(JsonDataWriter): return JsonDataWriter.Create(outputModule.Attributes);
            }
            return null;
        }
    }
}
