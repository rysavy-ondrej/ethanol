using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.DataObjects;

namespace Ethanol.ContextBuilder.Polishers
{
    public static class ContextPolisherCatalogEntry
    {
        /// <summary>
        /// Returns an observable transformer that polishes the IP host context with tags and transforms it into a host context.
        /// </summary>
        /// <param name="catalog">The context transform catalog.</param>
        /// <returns>An observable transformer that transforms the IP host context into a host context.</returns>
        public static IObservableTransformer<ObservableEvent<IpHostContextWithTags>, HostContext> GetContextPolisher(this ContextTransformCatalog catalog)
        {
            return new IpHostContextPolisher(catalog.Environment.Logger);
        }
        /// <summary>
        /// Gets the context polisher for IpHostContextWithTags.
        /// </summary>
        /// <param name="catalog">The ContextTransformCatalog.</param>
        /// <returns>The context polisher.</returns>
        public static IRefiner<ObservableEvent<IpHostContextWithTags>, HostContext> GetContextPolisher2(this ContextTransformCatalog catalog)
        {
            return new IpContextRefiner(null, catalog.Environment.Logger);
        }
    }
}
