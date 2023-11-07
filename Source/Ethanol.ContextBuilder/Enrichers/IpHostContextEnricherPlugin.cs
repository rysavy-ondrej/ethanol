using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers.TagProviders;
using Ethanol.ContextBuilder.Observable;
using System;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// A plugin to enrich IP host contexts with additional, known information.
    /// </summary>
    /// <remarks>
    /// This plugin uses the <see cref="IpHostContextEnricher"/> to enhance the context for IP hosts based on data 
    /// from a provided configuration. The enriched context then provides more detailed information related to IP hosts.
    /// </remarks>
    public class IpHostContextEnricherPlugin : IObservableTransformer
    {
        private IpHostContextEnricher _enricher;

        /// <summary>
        /// Initializes a new instance of the <see cref="IpHostContextEnricherPlugin"/> class.
        /// </summary>
        /// <param name="enricher">The context enricher that will be used to provide additional context to IP hosts.</param>
        public IpHostContextEnricherPlugin(IpHostContextEnricher enricher)
        {
            _enricher = enricher;
        }

        /// <summary>
        /// Gets the name of this transformer plugin.
        /// </summary>
        public string TransformerName => nameof(IpHostContextEnricherPlugin);

        /// <summary>
        /// Gets the source type for which this plugin is applicable.
        /// </summary>
        public Type SourceType => typeof(ObservableEvent<IpHostContext>);

        /// <summary>
        /// Gets the target type that will be produced after the transformation.
        /// </summary>
        public Type TargetType => typeof(ObservableEvent<RawHostContext>);

        /// <summary>
        /// Signals that the enrichment process has completed.
        /// </summary>
        public void OnCompleted()
        {
            _enricher.OnCompleted();
        }

        /// <summary>
        /// Signals that an error has occurred during the enrichment process.
        /// </summary>
        /// <param name="error">The exception that occurred.</param>
        public void OnError(Exception error)
        {
            _enricher.OnError(error);
        }

        /// <summary>
        /// Pushes the next enriched context value.
        /// </summary>
        /// <param name="value">The context to be enriched.</param>
        public void OnNext(object value)
        {
            _enricher.OnNext((ObservableEvent<IpHostContext>)value);
        }

        /// <summary>
        /// Subscribes an observer to the observable sequence.
        /// </summary>
        /// <param name="observer">The observer that will receive notifications from the observable sequence.</param>
        /// <returns>A disposable object that can be used to unsubscribe the observer from the observable sequence.</returns>
        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _enricher.Subscribe(observer);
        }
    }

}
