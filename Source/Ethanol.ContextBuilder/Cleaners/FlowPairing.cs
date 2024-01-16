using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Ethanol.DataObjects;
namespace Ethanol.ContextBuilder.Cleaners
{
    /// <summary>
    /// Represents a mechanism for pairing individual IP flows into bidirectional flows.
    /// This class identifies matching request and response flows based on their flow keys and
    /// pairs them together, producing a unified bidirectional flow.
    /// </summary>
    /// <remarks>
    /// Individual IP flows that come into the system may represent either a request or a response.
    /// The FlowPairing class attempts to find the matching counterpart for each flow and 
    /// pairs them together. If a matching flow is not found within a specified timeout,
    /// the single flow is emitted as-is.
    /// </remarks>
    public class FlowPairing : IObserver<IpFlow>, IObservable<IpFlow>
    {
        private readonly Subject<IpFlow> _flowSubject;
        private readonly TimeSpan _flowTimeout;
        private readonly Cache<FlowKey, IpFlow> _flowDictionary;
        private DateTimeOffset _currentTime = DateTime.MinValue;
        private TaskCompletionSource _tcs = new TaskCompletionSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowPairing"/> class, specifying the timeout
        /// for how long to wait for a matching flow.
        /// </summary>
        /// <param name="flowTimeout">The timeout duration for pairing.</param>

        public FlowPairing(TimeSpan flowTimeout)
        {
            _flowSubject = new Subject<IpFlow>();
            _flowTimeout = flowTimeout;
            _flowDictionary = new Cache<FlowKey, IpFlow>();
        }

        public Task Completed => _tcs.Task;

        /// <summary>
        /// Publishes the remaining single flows upon completion of the input stream.
        /// </summary>
        public void OnCompleted()
        {
            foreach (var flow in _flowDictionary.CleanupExpiredItems(DateTime.MaxValue))
            {
                _flowSubject.OnNext(flow);
            }
            _flowSubject.OnCompleted();
            _tcs.SetResult();
        }

        /// <summary>
        /// Propagates an error through the flow pairing observable.
        /// </summary>
        /// <param name="error">The exception to propagate.</param>
        public void OnError(Exception error)
        {
            _flowSubject.OnError(error);
        }

        /// <summary>
        /// Processes a new IP flow, attempting to pair it with a matching flow from the cache.
        /// If no match is found within the specified timeout, the flow is emitted as-is.
        /// </summary>
        /// <param name="flow">The new IP flow to process.</param>
        public void OnNext(IpFlow flow)
        {
            var reverseFlow = _flowDictionary.GetAndRemove(flow.FlowKey.GetReverseFlowKey());
            if (reverseFlow != null)
            {
                var reqFlow = flow.FlowType == FlowType.RequestFlow ? flow : reverseFlow;
                var resFlow = flow.FlowType == FlowType.ResponseFlow ? flow : reverseFlow;

                var biflow = PairFlows(reqFlow, resFlow);
                if (biflow != null)
                {
                    _flowSubject.OnNext(biflow);
                }
            }
            else
            {
                _flowDictionary.Set(flow.FlowKey, flow, flow.TimeStart + _flowTimeout);
            }
            if (flow.TimeStart > _currentTime) _currentTime = flow.TimeStart;
            foreach(var expiredFlow in _flowDictionary.CleanupExpiredItems(_currentTime))
            {
                _flowSubject.OnNext(expiredFlow);
            }
        }

        /// <summary>
        /// Pair flows to the resulting bidirectional flow respecting their extensions.
        /// <para/>
        /// This pairing always work as even if incomaptible flows are provided (eg., DNS and TLS) 
        /// the bidirectional IpFlow is produced.
        /// </summary>
        /// <param name="reqFlow">The request flow.</param>
        /// <param name="resFlow">The response flow.</param>
        /// <returns>Returns the bidirectional flow from the given pair of flows.</returns>
        private static IpFlow? PairFlows(IpFlow reqFlow, IpFlow resFlow)
        {
            return (reqFlow, resFlow) switch
            {
                (TlsFlow q, TlsFlow r) => FlowPairMapper.Map<TlsFlow>(new FlowPair<TlsFlow>(q, r)),
                (HttpFlow q, HttpFlow r) => FlowPairMapper.Map<HttpFlow>(new FlowPair<HttpFlow>(q, r)),
                (DnsFlow q, DnsFlow r) => FlowPairMapper.Map<DnsFlow>(new FlowPair<DnsFlow>(q, r)),
                (IpFlow q, IpFlow r) => FlowPairMapper.Map<IpFlow>(new FlowPair<IpFlow>(q, r)),
                _ => null
            };
        }

        /// <summary>
        /// Subscribes an observer to the flow pairing observable, allowing it to receive paired bidirectional flows.
        /// </summary>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns>A disposable object representing the subscription.</returns>
        public IDisposable Subscribe(IObserver<IpFlow> observer)
        {
            return _flowSubject.Subscribe(observer);
        }

        class Cache<TKey, TValue> where TValue : class where TKey : notnull
        {
            private readonly SortedDictionary<DateTimeOffset, List<TKey>> _timeline = new SortedDictionary<DateTimeOffset, List<TKey>>();
            private readonly IDictionary<TKey, CacheItem<TValue>> _cache = new Dictionary<TKey, CacheItem<TValue>>();

            public TValue? Get(TKey key)
            {
                // Check if the item is in the cache
                if (_cache.TryGetValue(key, out CacheItem<TValue>? item))
                {
                    // Return the cached item
                    return item.Value;
                }
                // The item is not in the cache
                return null;
            }
            internal TValue? GetAndRemove(TKey flowKey)
            {
                var item = Get(flowKey);
                if (item != null) _cache.Remove(flowKey);
                return item;
            }

            public void Set(TKey key, TValue value, DateTimeOffset expiresAt)
            {
                // Create a new cache item with the specified value and expiration time
                var item = new CacheItem<TValue>(value, expiresAt);

                if (_timeline.ContainsKey(expiresAt))
                {
                    _timeline[expiresAt].Add(key);
                }
                else
                {
                    _timeline.Add(expiresAt, new List<TKey>() { key });
                }
                // Add or update the item in the cache
                _cache[key] = item;
            }

            public IEnumerable<TValue> CleanupExpiredItems(DateTimeOffset currentDate)
            {
                var expiredItems = _timeline.TakeWhile(x => x.Key < currentDate).ToList();

                // Remove all expired items from the cache
                foreach (var expiredItem in expiredItems)
                {
                    var expDate = expiredItem.Key;
                    _timeline.Remove(expDate);
                    foreach (var expFlow in expiredItem.Value)
                    {
                        if (_cache.TryGetValue(expFlow, out CacheItem<TValue>? item))
                        {
                            _cache.Remove(expFlow);
                            yield return item.Value;
                        }
                    }
                }
            }

            private class CacheItem<T>
            {
                public T Value { get; }
                public DateTimeOffset ExpiresAt { get; }

                public CacheItem(T value, DateTimeOffset expiresAt)
                {
                    Value = value;
                    ExpiresAt = expiresAt;
                }
            }
        }
    }
}
