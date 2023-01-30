using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Readers
{
    public abstract class InputDataReader<TRecord> : IObservable<TRecord>
    {
        CancellationTokenSource _cts;
        Subject<TRecord> _subject;
        private Task _readingTask;

        protected InputDataReader()
        {
            _subject = new Subject<TRecord>();
            _cts = new CancellationTokenSource();
        }

        protected abstract void Open();

        protected abstract bool TryGetNextRecord(CancellationToken ct, out TRecord record);

        protected abstract void Close();

        public Task StartReading()
        {
            _readingTask = Task.Factory.StartNew(ReaderProcedure, _cts.Token);
            return _readingTask;
        }

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

        public IDisposable Subscribe(IObserver<TRecord> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
