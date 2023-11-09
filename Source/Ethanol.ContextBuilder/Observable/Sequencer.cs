using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

public class Sequencer<T> : IObservableTransformer<ObservableEvent<T>, ObservableEvent<IObservable<T>>>
{
    Subject<ObservableEvent<T>> _subject = new Subject<ObservableEvent<T>>();
    SortedList<DateTime, Queue<ObservableEvent<T>>> _elementBuffer = new SortedList<DateTime, Queue<ObservableEvent<T>>>();
    int _bufferedElements = 0;
    int _maxQueueLength = 128;
    TaskCompletionSource _tcs = new TaskCompletionSource();



    public PipelineNodeType NodeType => PipelineNodeType.Transformer;

    public Sequencer(int queueLength)
    {
        this._maxQueueLength = queueLength;
    }

    public Task Completed => _tcs.Task;

    public void OnCompleted()
    {
        foreach (var queue in _elementBuffer.Values)
        {
            while (queue.Count > 0)
            {
                _subject.OnNext(queue.Dequeue());
            }
        }
        _subject.OnCompleted();
        _tcs.SetResult();
    }

    public void OnError(Exception error)
    {
        _subject.OnError(error);
    }

    public void OnNext(ObservableEvent<T> item)
    {
        if (!_elementBuffer.ContainsKey(item.StartTime))
        {
            _elementBuffer[item.StartTime] = new Queue<ObservableEvent<T>>();
        }
        _elementBuffer[item.StartTime].Enqueue(item);
        _bufferedElements++;

        // If the buffer exceeds the specified length, dequeue the earliest item.
        if (_bufferedElements > _maxQueueLength)
        {
            var oldest = _elementBuffer.First();
            _subject.OnNext(oldest.Value.Dequeue());
            _bufferedElements--;
            if (oldest.Value.Count == 0)
            {
                _elementBuffer.Remove(oldest.Key);
            }
        }
    }

    public IDisposable Subscribe(IObserver<ObservableEvent<IObservable<T>>> observer)
    {
        throw new NotImplementedException();
    }
}

