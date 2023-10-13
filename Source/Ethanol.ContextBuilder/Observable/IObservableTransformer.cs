using System;

namespace Ethanol.ContextBuilder.Observable
{
    /// <summary>
    /// An interface for all transformes. It stands for the untyped transformer that can be easily pipelined.
    /// <para/>
    /// For the stornlgy types version use <see cref="IObservableTransformer{TSource, TTarget}"/> interface instead.
    /// </summary>
    public interface IObservableTransformer : IObserver<object>, IObservable<object>
    {
        string TransformerName { get; }
        Type SourceType { get; }
        Type TargetType { get; }
    }
    /// <summary>
    /// An interface of all context builder. 
    /// It is observer for source data and observable for computed context data.
    /// </summary>
    /// <typeparam name="TSource">The type of source data.</typeparam>
    /// <typeparam name="TTarget">The type of generated context.</typeparam>
    public interface IObservableTransformer<in TSource, out TTarget> : IObserver<TSource>, IObservable<TTarget> { }
}
