using Ethanol.DataObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace Ethanol.ContextBuilder.Enrichers
{
    public class IpHostContextEnricher : IEnricher<TimeRange<IpHostContext>?, TimeRange<IpHostContextWithTags>>
    {
        private readonly PerformanceCounters? _counters;
        private readonly ILogger? _logger;
        private readonly ITagDataProvider<TagObject, IpHostContext> _tagProvider;

        public IpHostContextEnricher(ITagDataProvider<TagObject, IpHostContext> tagProvider, PerformanceCounters? counters = null, ILogger? logger = null)
        {
            _counters = counters;
            _logger = logger;
            _tagProvider = tagProvider;
        }

        public TimeRange<IpHostContextWithTags>? Enrich(TimeRange<IpHostContext> value)
        {
            try
            {
                Stopwatch sw = new();
                sw.Start();
                if (_counters!= null) _counters.InputCount++;
                var tags = _tagProvider.GetTags(value);
                sw.Stop();
                
                if (_counters!= null) _counters.RecordOperationTime(sw.ElapsedMilliseconds);
                
                var result = new TimeRange<IpHostContextWithTags>(new IpHostContextWithTags { HostAddress = value.Value?.HostAddress, Flows = value.Value?.Flows, Tags = tags.ToArray() }, 
                    value.StartTime, 
                    value.EndTime);
                
                if (_counters!= null)  _counters.OutputCount++;
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error in context enrichment. Value: {value}");
                if (_counters!= null) _counters.Errors++;
                return null;
            }
        }

        public class PerformanceCounters
        {
            public long InputCount;
            public long OutputCount;
            public int Errors;
            private long OperationMaxTime;
            private long OperationMinTime;
            private long OperationAverageTime;

            internal void RecordOperationTime(long elapsedMilliseconds)
            {
                OperationMaxTime = System.Math.Max(OperationMaxTime, elapsedMilliseconds);
                OperationMinTime = System.Math.Min(OperationMinTime, elapsedMilliseconds);
                OperationAverageTime = (OperationAverageTime * 9 + elapsedMilliseconds) / 10;
            }
        }
    }
}
