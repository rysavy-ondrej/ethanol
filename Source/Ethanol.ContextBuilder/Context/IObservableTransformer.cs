using System;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// An interface of all context builder. 
    /// It is observer for source data and observable for computed context data.
    /// </summary>
    /// <typeparam name="TSource">The type of source data.</typeparam>
    /// <typeparam name="TTarget">The type of generated context.</typeparam>
    public interface IObservableTransformer<in TSource, out TTarget> : IObserver<TSource>, IObservable<TTarget> { }
}
