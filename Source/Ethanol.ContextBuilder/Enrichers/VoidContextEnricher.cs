using System;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents a no-op context enricher that does not modify or enrich the incoming data.
    /// This class is intended for use in situations where a placeholder or no-action operation is required in a processing pipeline.
    /// </summary>
    public class VoidContextEnricher<TSource, TTarget> : IEnricher<TSource, TTarget>
    {
        private readonly Func<TSource, TTarget> _transform;
        private readonly PerformanceCounters? _counters;
        /// <summary>
        /// Represents a void context enricher that transforms a source object to a target object using a specified transform function.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        public VoidContextEnricher(Func<TSource, TTarget> transform, PerformanceCounters? counters = null)
        {
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
            _counters = counters;
        }

        /// <summary>
        /// Enriches the input item and returns the enriched value.
        /// </summary>
        /// <param name="item">The input item to be enriched.</param>
        /// <returns>The enriched value of the input item.</returns>
        public TTarget? Enrich(TSource item)
        {
            if (_counters != null) _counters.InputCount++;
           
            var newValue = _transform(item); 

            if (_counters != null) _counters.OutputCount++;
            
            return newValue; 
        }

        /// <summary>
        /// Represents performance counters for tracking input and output counts.
        /// </summary>
        public class PerformanceCounters
        {
            /// <summary>
            /// Gets or sets the number of inputs.
            /// </summary>
            public double InputCount;
            /// <summary>
            /// Gets or sets the output count.
            /// </summary>
            public double OutputCount;
        }
    }
}
