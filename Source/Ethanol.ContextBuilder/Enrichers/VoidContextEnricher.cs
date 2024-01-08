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

        public VoidContextEnricher(Func<TSource, TTarget> transform)
        {

            this._transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public TTarget Enrich(TSource item)
        {
            if (_counters != null) _counters.InputCount++;
           
            var newValue = _transform(item); 

            if (_counters != null) _counters.OutputCount++;
            
            return newValue; 
        }

        class PerformanceCounters
        {
            public double InputCount;
            public double OutputCount;
        }
    }
}
