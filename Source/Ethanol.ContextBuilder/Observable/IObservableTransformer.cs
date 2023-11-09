using Ethanol.ContextBuilder.Pipeline;
using System;

namespace Ethanol.ContextBuilder.Observable
{
    /// <summary>
    /// Defines the contract for a transformer that observes and produces objects, enabling the transformation of data streams.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface are expected to observe data of a specific source type and 
    /// produce transformed data of a specific target type, effectively acting as a transformation step 
    /// in an observable data pipeline.
    /// </remarks>
    public interface IObservableTransformer : IObserver<object>, IObservable<object>, IPipelineNode
    {
        /// <summary>
        /// Gets the name of the transformer, typically used for identification or logging purposes.
        /// </summary>
        string TransformerName { get; }

        /// <summary>
        /// Gets the type of the source data that the transformer is capable of observing and processing.
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// Gets the type of the data that the transformer produces after transformation.
        /// </summary>
        Type TargetType { get; }
    }

    /// <summary>
    /// An interface of all context builder. 
    /// It is observer for source data and observable for computed context data.
    /// </summary>
    /// <typeparam name="TSource">The type of source data.</typeparam>
    /// <typeparam name="TTarget">The type of generated context.</typeparam>
    public interface IObservableTransformer<in TSource, out TTarget> : IObserver<TSource>, IObservable<TTarget>, IPipelineNode { }
}
