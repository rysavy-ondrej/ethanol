using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Enriches the context of IP hosts by applying additional data from provided data sources.
    /// </summary>
    public class IpHostContextEnricher : IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>>
    {
        /// <summary>
        /// The logger used for logging purposes.
        /// </summary>
        private readonly ILogger? _logger;
        /// Represents a subject that can both observe items of type <see cref="ObservableEvent<RawHostContext>"/> 
        /// as well as produce them. This is often used to represent both the source and terminator of an observable sequence.
        private readonly Subject<ObservableEvent<IpHostContextWithTags>> _subject;

        /// Provides a mechanism for querying tag data. It can be used to retrieve or fetch tags associated with certain data or context.
        private readonly ITagDataProvider<TagObject, IpHostContext> _tagProvider;

        /// Represents a mechanism for signaling the completion of some asynchronous operation. 
        /// This provides a way to manually control the lifetime of a Task, signaling its completion.
        private TaskCompletionSource _tcs = new TaskCompletionSource();

        private PerformanceCounters _counters = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="IpHostContextEnricher"/> class.
        /// </summary>
        /// <param name="tagQueryable">Provides querying capabilities for tags.</param>
        public IpHostContextEnricher(ITagDataProvider<TagObject, IpHostContext> tagProvider, ILogger? logger = null)
        {
            _subject = new Subject<ObservableEvent<IpHostContextWithTags>>();
            _tagProvider = tagProvider;
            _logger = logger;
        }

        /// <summary>
        /// Gets a task that represents the completion of the operation.
        /// </summary>
        public Task Completed => _tcs.Task;

        
        public IPerformanceCounters Counters => _counters;

        /// <summary>
        /// Signals that the enrichment process has completed.
        /// </summary>
        public void OnCompleted()
        {
            _subject.OnCompleted();
            _tcs.SetResult();
        }
        /// <summary>
        /// Notifies observers that an exception has occurred.
        /// </summary>
        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }
        /// <summary>
        /// Enriches the provided <see cref="IpHostContext"/> and notifies observers of the enriched context.
        /// </summary>
        public void OnNext(ObservableEvent<IpHostContext> value)
        {
            try
            {
                Stopwatch sw = new();
                sw.Start();
                _counters.InputCount++;
                var tags = _tagProvider.GetTags(value);
                sw.Stop();
                _counters.RecordOperationTime(sw.ElapsedMilliseconds);
                _subject.OnNext(new ObservableEvent<IpHostContextWithTags>(new IpHostContextWithTags { HostAddress = value.Payload?.HostAddress, Flows = value.Payload?.Flows, Tags = tags.ToArray() }, value.StartTime, value.EndTime));
                _counters.OutputCount++;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error in context enrichment. Value: {value}");
                _subject.OnError(ex);
            }
        }

        /// <summary>
        /// Subscribes an observer to the enriched context.
        /// </summary>
        /// <param name="observer">The observer that will receive notifications.</param>
        /// <returns>Disposable object representing the subscription.</returns>
        public IDisposable Subscribe(IObserver<ObservableEvent<IpHostContextWithTags>> observer)
        {
            return _subject.Subscribe(observer);
        }

        class PerformanceCounters : IPerformanceCounters
        {
            public double InputCount;

            public double OutputCount;

            public double OperationMaxTime;

            public double OperationMinTime;
            
            public double OperationAverageTime;

            public string Name => nameof(IpHostContextEnricher);

            public string Category => "Performance Counters";

            public IEnumerable<string> Keys => new [] { nameof(InputCount), nameof(OutputCount), nameof(OperationMinTime), nameof(OperationMaxTime), nameof(OperationAverageTime)};

            public int Count => 5;

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out double value)
            {
                if (key == null) {  throw new ArgumentNullException(nameof(key)); }
                switch(key)
                {
                    case nameof(InputCount):
                        value = InputCount;
                        return true;
                    case nameof(OutputCount):
                        value = OutputCount;
                        return true;
                    case nameof(OperationMinTime):
                        value = OperationMinTime;
                        return true;
                    case nameof(OperationMaxTime):
                        value = OperationMaxTime;
                        return true;
                    case nameof(OperationAverageTime):
                        value = OperationAverageTime;
                        return true;
                }
                value = 0.0;
                return false;
            }

            internal void RecordOperationTime(long elapsedMilliseconds)
            {
                OperationMaxTime = System.Math.Max(OperationMaxTime, elapsedMilliseconds);
                OperationMinTime = System.Math.Min(OperationMinTime, elapsedMilliseconds);
                OperationAverageTime = (OperationAverageTime * 9 + elapsedMilliseconds) / 10;
            }
        }
    }
}
