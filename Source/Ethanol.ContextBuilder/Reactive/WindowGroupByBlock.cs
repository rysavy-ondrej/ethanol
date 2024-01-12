using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

/// <summary>
/// Represents a dataflow block that groups elements into windows based on a key selector function.
/// </summary>
/// <typeparam name="T">The type of the elements being grouped.</typeparam>
/// <typeparam name="K">The type of the key used for grouping.</typeparam>
/// <typeparam name="V">The type of the values in the groups.</typeparam>
public class WindowGroupByBlock<T, K, V> : IPropagatorBlock<Timestamped<T>, Timestamped<IGrouping<K, V>>>  where K : notnull
{
    private readonly Func<T, K> _keySelector;
    private readonly Func<T, V> _valueSelector;
    private readonly Func<KeyValuePair<K,IEnumerable<V>>>? _eowSelector;
    private readonly TimeSpan _windowSize;
    private readonly int _maxHosts;
    private readonly BufferBlock<Timestamped<IGrouping<K, V>>> _outputBuffer;

    private readonly ActionBlock<Timestamped<T>> _inputBlock;

    public WindowGroupByBlock(Func<T, K> keySelector, Func<T,V> valSelector, Func<KeyValuePair<K,IEnumerable<V>>>? eowSelector, TimeSpan windowSize, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
    {
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _valueSelector = valSelector ?? throw new ArgumentNullException(nameof(valSelector));
        _eowSelector = eowSelector;
        _windowSize = windowSize;
        _outputBuffer = new BufferBlock<Timestamped<IGrouping<K, V>>>(executionDataflowBlockOptions);
        _inputBlock = new ActionBlock<Timestamped<T>>(ProcessMessageAsync);
        _maxHosts = executionDataflowBlockOptions.BoundedCapacity-1;
    }

    public Task Completion => _outputBuffer.Completion;

    public int KeyCount => _hostDictionary.Count;

    public int ValueCount => _allFlowsCount;

    public void Complete()
    {
        _inputBlock.Complete();
        FlushGroupTableInternal();
        _outputBuffer.Complete();
    }

   
   public void Fault(Exception exception)
   {
       ((IDataflowBlock)_outputBuffer).Fault(exception);
   }

   public Timestamped<IGrouping<K, V>> ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Timestamped<IGrouping<K, V>>> target, out bool messageConsumed)
   {
       return ((ISourceBlock<Timestamped<IGrouping<K, V>>>)_outputBuffer).ConsumeMessage(messageHeader, target, out messageConsumed);
   }

   public IDisposable LinkTo(ITargetBlock<Timestamped<IGrouping<K, V>>> target, DataflowLinkOptions linkOptions)
   {
       return _outputBuffer.LinkTo(target, linkOptions);
   }

   public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Timestamped<IGrouping<K, V>>> target)
   {
       ((ISourceBlock<Timestamped<IGrouping<K, V>>>)_outputBuffer).ReleaseReservation(messageHeader, target);
   }

   public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Timestamped<IGrouping<K, V>>> target)
   {
       return ((ISourceBlock<Timestamped<IGrouping<K, V>>>)_outputBuffer).ReserveMessage(messageHeader, target);
   }

   readonly Dictionary<K,LinkedList<V>> _hostDictionary = new Dictionary<K, LinkedList<V>>();
   int _allFlowsCount = 0;
   DateTimeOffset? _windowStart = null;


   private async Task CompleteWindowAsync(DateTimeOffset timestamp)
   {

       await FlushGroupTableInternal();
       _windowStart = GetWindowStart(timestamp);
   }

   private async Task ProcessMessageAsync(Timestamped<T> message)
   {
       _windowStart ??= GetWindowStart(message.Timestamp);
       if (message.Timestamp > _windowStart + _windowSize)
       {
           await CompleteWindowAsync(message.Timestamp);
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
           else if(_maxHosts == -1 || _hostDictionary.Count < _maxHosts)
           {
               list = new LinkedList<V>();
               list.AddLast(_valueSelector(messageValue.Value));
               _hostDictionary[_keySelector(messageValue.Value)] = list;
           }
       }
   }

    private async Task FlushGroupTableInternal()
    {
        lock (_lockObject)
        {
            foreach (var item in _hostDictionary)
            {
                var group = new Timestamped<IGrouping<K, V>>(new InternalGrouping<K, V>(item.Key, item.Value), _windowStart ?? DateTimeOffset.MinValue);
                _outputBuffer.Post(group);
            }
            _hostDictionary.Clear();
            _allFlowsCount = 0;
        }
        if (_eowSelector != null)
        {
            var eow = _eowSelector();
            await _outputBuffer.SendAsync(new Timestamped<IGrouping<K, V>>(new InternalGrouping<K, V>(eow.Key, eow.Value), _windowStart ?? DateTimeOffset.MinValue));
        }
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
