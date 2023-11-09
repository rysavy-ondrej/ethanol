using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

/// <summary>
/// Transforms an observable sequence by ordering events based on their start time.
/// This sequencer ensures that the observable events are emitted in a chronological sequence,
/// even if they arrive out of order, up to a specified queue length.
/// </summary>
/// <typeparam name="T">The type of data contained within the observable events.</typeparam>
/// <remarks>
/// The <c>SequencerTransformer</c> uses a sorted buffer to temporarily hold and sort events
/// until they can be released in order. This can be particularly useful in systems where
/// events are not guaranteed to arrive in the order they occurred, such as in distributed systems
/// or applications with parallel processing.
/// </remarks>
public class SequencerTransformer<T> : IObservableTransformer<ObservableEvent<T>, ObservableEvent<T>>
{
    // The subject to which sorted events will be emitted.
    private Subject<ObservableEvent<T>> _subject = new Subject<ObservableEvent<T>>();

    // Buffer to hold events sorted by their start time.
    private SortedList<DateTime, Queue<ObservableEvent<T>>> _elementBuffer = new SortedList<DateTime, Queue<ObservableEvent<T>>>();

    // Counter to keep track of the total number of elements buffered.
    private int _bufferedElements = 0;

    // The maximum number of events the queue will hold before starting to emit items.
    private int _maxQueueLength;

    // TaskCompletionSource to signal completion of event processing.
    private TaskCompletionSource _tcs = new TaskCompletionSource();

    /// <summary>
    /// Gets the type of the pipeline node, which in this case is a transformer.
    /// </summary>
    public PipelineNodeType NodeType => PipelineNodeType.Transformer;

    /// <summary>
    /// Initializes a new instance of the SequencerTransformer class with a specified queue length.
    /// </summary>
    /// <param name="queueLength">The maximum length of the queue before the transformer starts emitting events.</param>
    public SequencerTransformer(int queueLength)
    {
        this._maxQueueLength = queueLength;
    }

    /// <summary>
    /// Gets the task that completes when the sequence has finished processing.
    /// </summary>
    public Task Completed => _tcs.Task;

    /// <summary>
    /// Signals that the event sequence is completed and releases any buffered events in order.
    /// </summary>
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

    /// <summary>
    /// Propagates an error through the transformer.
    /// </summary>
    /// <param name="error">The exception that occurred.</param>
    public void OnError(Exception error)
    {
        _subject.OnError(error);
    }

    /// <summary>
    /// Processes the next observable event, buffering it until it can be released in order.
    /// </summary>
    /// <param name="item">The observable event to process.</param>
    public void OnNext(ObservableEvent<T> item)
    {
        if (!_elementBuffer.ContainsKey(item.StartTime))
        {
            _elementBuffer[item.StartTime] = new Queue<ObservableEvent<T>>();
        }
        _elementBuffer[item.StartTime].Enqueue(item);
        _bufferedElements++;

        // Emit the earliest event if the buffer exceeds the maximum allowed length.
        if (_bufferedElements > _maxQueueLength)
        {
            var oldest = _elementBuffer.First();
            _subject.OnNext(oldest.Value.Dequeue());
            _bufferedElements--;
            // Clean up the queue if it's empty to save memory.
            if (oldest.Value.Count == 0)
            {
                _elementBuffer.Remove(oldest.Key);
            }
        }
    }
    /// <summary>
    /// Subscribes an observer to the transformer to receive ordered observable events.
    /// </summary>
    /// <param name="observer">The observer that wants to receive events.</param>
    /// <returns>A disposable that can be used to unsubscribe the observer.</returns>
    public IDisposable Subscribe(IObserver<ObservableEvent<T>> observer)
    {
        return _subject.Subscribe(observer);
    }
}

