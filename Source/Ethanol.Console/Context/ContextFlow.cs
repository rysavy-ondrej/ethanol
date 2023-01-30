using Ethanol.ContextBuilder.Classifiers;
using System;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Represents a span of time window in which the context information is computed. 
    /// Validity of context flow is determined by this window.
    /// </summary>
    /// <param name="Start">The start of the window.</param>
    /// <param name="Duration">The duration of the window.</param>
    public record WindowSpan(DateTime Start, TimeSpan Duration)
    {
        internal static WindowSpan FromLong(long startTime, long endTime)
        {
            return new WindowSpan(new DateTime(startTime), new TimeSpan(endTime - startTime));
        }
    }

    /// <summary>
    /// the context flow object.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="Id"></param>
    /// <param name="FlowKey"></param>
    /// <param name="Window"></param>
    /// <param name="Context"></param>
    public record ContextFlow<TContext>(string Id, FlowKey FlowKey, WindowSpan Window, TContext Context);

    /// <summary>
    /// This context flow is used internally during computations in streams.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="FlowKey"></param>
    /// <param name="Context"></param>
    public record InternalContextFlow<TContext>(FlowKey FlowKey, TContext Context);
    public record ClassifiedContextFlow<TContext>(FlowKey FlowKey, ClassificationResult[] Tags, TContext Context);
}
