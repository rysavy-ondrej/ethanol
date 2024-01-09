using Ethanol.DataObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents an enricher that adds tags to the IP host context.
    /// </summary>
    public class IpHostContextEnricher : IEnricher<TimeRange<IpHostContext>, TimeRange<IpHostContextWithTags>>
    {
        private readonly PerformanceCounters? _counters;
        private readonly ILogger? _logger;
        private readonly ITagDataProvider<TagObject, IpHostContext> _tagProvider;

        /// <summary>
        /// Represents an enricher that adds IP host context to log events.
        /// </summary>
        /// <param name="tagProvider">The tag data provider.</param>
        /// <param name="counters">The performance counters.</param>
        /// <param name="logger">The logger.</param>
        public IpHostContextEnricher(ITagDataProvider<TagObject, IpHostContext> tagProvider, PerformanceCounters? counters = null, ILogger? logger = null)
        {
            _counters = counters;
            _logger = logger;
            _tagProvider = tagProvider;
        }

        /// <summary>
        /// Enriches the provided <see cref="TimeRange{IpHostContext}"/> value with tags and returns a new <see cref="TimeRange{IpHostContextWithTags}"/> object.
        /// </summary>
        /// <param name="value">The <see cref="TimeRange{IpHostContext}"/> value to be enriched.</param>
        /// <returns>A new <see cref="TimeRange{IpHostContextWithTags}"/> object with enriched data, or null if an error occurs during enrichment.</returns>
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

        /// <summary>
        /// Represents a set of performance counters for tracking input and output counts, errors, and operation times.
        /// </summary>
        public class PerformanceCounters
        {
            /// <summary>
            /// Gets or sets the input count.
            /// </summary>
            public long InputCount;
            /// <summary>
            /// Gets or sets the output count.
            /// </summary>
            public long OutputCount;
            /// <summary>
            /// Gets or sets the number of errors.
            /// </summary>
            public int Errors;
            /// <summary>
            /// Gets or sets the maximum time allowed for an operation.
            /// </summary>
            private long OperationMaxTime;
            /// <summary>
            /// Represents the minimum time for an operation.
            /// </summary>
            private long OperationMinTime;
            /// <summary>
            /// Represents the average time taken for an operation.
            /// </summary>
            private long OperationAverageTime;

            /// <summary>
            /// Records the operation time and updates the maximum, minimum, and average operation times.
            /// </summary>
            /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
            internal void RecordOperationTime(long elapsedMilliseconds)
            {
                OperationMaxTime = System.Math.Max(OperationMaxTime, elapsedMilliseconds);
                OperationMinTime = System.Math.Min(OperationMinTime, elapsedMilliseconds);
                OperationAverageTime = (OperationAverageTime * 9 + elapsedMilliseconds) / 10;
            }
        }
    }
}
