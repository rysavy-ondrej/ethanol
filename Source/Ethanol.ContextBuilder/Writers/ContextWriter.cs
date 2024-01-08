using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Represents a base class for all context writers. A writer outputs 
    /// context-based data in a specific output format. 
    /// </summary>
    /// <typeparam name="TRecord">The type of record the context writer will handle.</typeparam>
    public abstract class ContextWriter<TRecord> : IDataWriter<TRecord>
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
            _counters.InputCount++;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            if (!_isopen) { Open(); _isopen = true; }
            Write(value);
            sw.Stop();
            _counters.RecordOperationTime(sw.ElapsedMilliseconds);
            _counters.WrittenCount++;
        }

        /// <summary>
        /// Gets a task that signals the completion of the writing operation.
        /// </summary>
        public Task Completed => _taskCompletionSource.Task;

        public Type RecordType => typeof(TRecord);

        protected PerformanceCounters _counters = new PerformanceCounters();
        public IPerformanceCounters Counters => _counters;

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

        protected class PerformanceCounters : IPerformanceCounters
        {
            public double InputCount { get; set; }
            public double WrittenCount { get; set; }

            public double WriteMaxTime;

            public double WriteMinTime;
            
            public double WriteAverageTime;
            public string Name => "ContextWriter";

            public string Category => "Performance Counters";

            public IEnumerable<string> Keys => new [] { nameof(InputCount), nameof(WrittenCount), nameof(WriteMinTime), nameof(WriteMaxTime), nameof(WriteAverageTime)};

            public int Count => 5;

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out double value)
            {
                if (key == null) {  throw new ArgumentNullException(nameof(key)); }
                switch(key)
                {
                    case nameof(InputCount):
                        value = InputCount;
                        return true;
                    case nameof(WrittenCount):
                        value = WrittenCount;
                        return true;
                    case nameof(WriteMinTime):
                        value = WriteMinTime;
                        return true;
                    case nameof(WriteMaxTime):
                        value = WriteMaxTime;
                        return true;
                    case nameof(WriteAverageTime):
                        value = WriteAverageTime;
                        return true;
                }
                value = 0.0;
                return false;
            }
            
            internal void RecordOperationTime(long elapsedMilliseconds)
            {
                WriteMaxTime = System.Math.Max(WriteMaxTime, elapsedMilliseconds);
                WriteMinTime = System.Math.Min(WriteMinTime, elapsedMilliseconds);
                WriteAverageTime = (WriteAverageTime * 9 + elapsedMilliseconds) / 10;
            }
        }
    }
}
