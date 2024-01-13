using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;



/// <summary>
/// Represents a batch of items in a window.
/// </summary>
/// <typeparam name="T">The type of items in the batch.</typeparam>
public readonly struct Batch<T>
{
    /// <summary>
    /// Gets the array of items in the batch.
    /// </summary>
    public T[] Items { get; }

    /// <summary>
    /// Gets the tick start value.
    /// </summary>
    public long TickStart { get; }

    /// <summary>
    /// Gets the duration of the window group.
    /// </summary>
    public long Duration { get; }

    /// <summary>
    /// Gets a value indicating whether this batch is the last batch in the window.
    /// </summary>
    public bool Last { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Batch{T}"/> struct.
    /// </summary>
    /// <param name="items">The array of items in the batch.</param>
    /// <param name="last">A value indicating whether this batch is the last batch.</param>
    public Batch(T[] items, long tickStart, long duration, bool last)
    {
        Items = items;
        TickStart = tickStart;
        Duration = duration;
        Last = last;
    }

}
public static class Batch
{
    /// <summary>
    /// Creates a new instance of the <see cref="Batch{T}"/> struct.
    /// </summary>
    /// <typeparam name="T">The type of items in the batch.</typeparam>
    /// <param name="items">The array of items in the batch.</param>
    /// <param name="last">A value indicating whether this batch is the last batch.</param>
    /// <returns>A new instance of the <see cref="Batch{T}"/> struct.</returns>
    public static Batch<T> Create<T>(T[] items, long tickStart, long duration, bool last)
    {
        return new Batch<T>(items, tickStart, duration, last);
    }
}

/// <summary>
/// Represents a dataflow block that groups elements into windows based on a key selector function.
/// </summary>
/// <typeparam name="T">The type of the elements being grouped.</typeparam>
/// <typeparam name="K">The type of the key used for grouping.</typeparam>
/// <typeparam name="V">The type of the values in the groups.</typeparam>
public class WindowGroupByBlock<T, K, V> : IPropagatorBlock<Timestamped<T>, Batch<IGrouping<K, V>>> where K : notnull
{
    private readonly Func<T, K> _keySelector;
    private readonly Func<T, V> _valueSelector;
    private readonly TimeSpan _windowSize;
    private readonly int _batchSize;
    private readonly int _maxHosts;
    private Dictionary<K, LinkedList<V>> _hostDictionary = new Dictionary<K, LinkedList<V>>();
    private int _allFlowsCount = 0;
    private DateTimeOffset? _windowStart = null;
    DateTimeOffset? _lastFlowTimestamp = null;

    public DateTime? CurrentWindowStart => _windowStart != null ? new DateTime(_windowStart.Value.Ticks) : null;

    public DateTime? NextWindowStart => _windowStart != null ? new DateTime(_windowStart.Value.Ticks + _windowSize.Ticks) : null;

    public DateTime? CurrentTime => _lastFlowTimestamp != null ? new DateTime(_lastFlowTimestamp.Value.Ticks) : null;

    private readonly TransformManyBlock<Timestamped<Dictionary<K, LinkedList<V>>>, Batch<IGrouping<K, V>>> _outputBuffer;

    private readonly ActionBlock<Timestamped<T>> _inputBlock;

    public WindowGroupByBlock(Func<T, K> keySelector, Func<T, V> valSelector, TimeSpan windowSize, int batchSize, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
    {
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _valueSelector = valSelector ?? throw new ArgumentNullException(nameof(valSelector));
        _windowSize = windowSize;
        this._batchSize = batchSize;
        _outputBuffer = new TransformManyBlock<Timestamped<Dictionary<K, LinkedList<V>>>, Batch<IGrouping<K, V>>>(EmitWindow, executionDataflowBlockOptions);
        _inputBlock = new ActionBlock<Timestamped<T>>(ProcessMessageAsync);
        _maxHosts = executionDataflowBlockOptions.BoundedCapacity;
    }

    /// <summary>
    /// Emits windows of batches containing timestamped groupings based on the provided dictionary.
    /// </summary>
    /// <param name="dictionary">The timestamped dictionary containing the groupings.</param>
    /// <returns>An enumerable of batches containing timestamped groupings.</returns>
    private IEnumerable<Batch<IGrouping<K, V>>> EmitWindow(Timestamped<Dictionary<K, LinkedList<V>>> dictionary)
    {
        var duration = _windowSize.Ticks;
        var windowStart = dictionary.Timestamp.Ticks;
        IGrouping<K, V>[]? array = null; 
        foreach(var chunk in dictionary.Value.AsEnumerable().Chunk(_batchSize))
        {
            if (array != null)
            {
                yield return Batch.Create(array, windowStart, duration, false);
            }
            array = chunk.Select(item =>(IGrouping<K,V>)new InternalGrouping<K, V>(item.Key, item.Value)).ToArray();
        }
        if (array != null)
        {
            yield return Batch.Create(array, windowStart, duration, true);
        }
    }

    public Task Completion => _outputBuffer.Completion;

    public int KeyCount => _hostDictionary.Count;

    public int ValueCount => _allFlowsCount;

    public void Complete()
    {
        _inputBlock.Complete();
        FlushGroupTableInternal().ConfigureAwait(false).GetAwaiter().GetResult();
        _outputBuffer.Complete();
    }


    public void Fault(Exception exception)
    {
        ((IDataflowBlock)_outputBuffer).Fault(exception);
    }

    public Batch<IGrouping<K, V>> ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Batch<IGrouping<K, V>>> target, out bool messageConsumed)
    {
        return ((ISourceBlock<Batch<IGrouping<K, V>>>)_outputBuffer).ConsumeMessage(messageHeader, target, out messageConsumed);
    }

    public IDisposable LinkTo(ITargetBlock<Batch<IGrouping<K, V>>> target, DataflowLinkOptions linkOptions)
    {
        return _outputBuffer.LinkTo(target, linkOptions);
    }

    public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Batch<IGrouping<K, V>>> target)
    {
        ((ISourceBlock<Batch<IGrouping<K, V>>>)_outputBuffer).ReleaseReservation(messageHeader, target);
    }

    public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Batch<IGrouping<K, V>>> target)
    {
        return ((ISourceBlock<Batch<IGrouping<K, V>>>)_outputBuffer).ReserveMessage(messageHeader, target);
    }

    private async Task CompleteWindowAsync(DateTimeOffset timestamp)
    {

        await FlushGroupTableInternal();
        _windowStart = GetWindowStart(timestamp);
    }

    private async Task ProcessMessageAsync(Timestamped<T> message)
    {
        _windowStart ??= GetWindowStart(message.Timestamp);
        _lastFlowTimestamp ??= message.Timestamp;

        if (message.Timestamp > _windowStart + _windowSize)
        {
            await CompleteWindowAsync(message.Timestamp);
        }
        if (_lastFlowTimestamp < message.Timestamp)
        {
            _lastFlowTimestamp = message.Timestamp;
        }
        AddElementInternal(message);
    }

    private readonly object _lockObject = new();
    private void AddElementInternal(Timestamped<T> messageValue)
    {
        lock (_lockObject)
        {
            _allFlowsCount++;
            if (_hostDictionary.TryGetValue(_keySelector(messageValue.Value), out var list))
            {
                list.AddLast(_valueSelector(messageValue.Value));
            }
            else if (_maxHosts == -1 || _hostDictionary.Count < _maxHosts)
            {
                list = new LinkedList<V>();
                list.AddLast(_valueSelector(messageValue.Value));
                _hostDictionary[_keySelector(messageValue.Value)] = list;
            }
        }
    }

    private async Task FlushGroupTableInternal()
    {
        Dictionary<K, LinkedList<V>>? dictionary = null;
        lock (_lockObject)
        {
            dictionary = _hostDictionary;
            _hostDictionary = new Dictionary<K, LinkedList<V>>();
            _allFlowsCount = 0;
        }
        await _outputBuffer.SendAsync(Timestamped.Create(dictionary, _windowStart ?? DateTimeOffset.MinValue));
    }
    DateTimeOffset GetWindowStart(DateTimeOffset timestamp)
    {
        var timeOfDayTicks = timestamp.TimeOfDay.Ticks;
        var windowTicks = _windowSize.Ticks % TimeSpan.FromDays(1).Ticks;   //this is for sure that we are within a single day
        var shiftTicks = timeOfDayTicks % windowTicks;
        var windowStart = timestamp - TimeSpan.FromTicks(shiftTicks);
        return windowStart;
    }

    public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, Timestamped<T> messageValue, ISourceBlock<Timestamped<T>>? source, bool consumeToAccept)
    {
        return ((ITargetBlock<Timestamped<T>>)_inputBlock).OfferMessage(messageHeader, messageValue, source, consumeToAccept);
    }

    private struct InternalGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly IEnumerable<TElement> _elements;

        public TKey Key { get; }

        public InternalGrouping(TKey key, IEnumerable<TElement> elements)
        {
            Key = key;
            _elements = elements;
        }

        public IEnumerator<TElement> GetEnumerator() => _elements.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _elements.GetEnumerator();
    }
}
