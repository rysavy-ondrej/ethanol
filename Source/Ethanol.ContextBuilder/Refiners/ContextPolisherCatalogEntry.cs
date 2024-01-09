using Ethanol.Catalogs;
using Ethanol.DataObjects;

namespace Ethanol.ContextBuilder.Polishers
{
    public static class ContextPolisherCatalogEntry
    {
        /// <summary>
        /// Gets the context polisher for IpHostContextWithTags.
        /// </summary>
        /// <param name="catalog">The ContextTransformCatalog.</param>
        /// <returns>The context polisher.</returns>
        public static IRefiner<TimeRange<IpHostContextWithTags>, HostContext> GetContextPolisher2(this ContextBuilderCatalog catalog)
        {
            return new IpHostContextRefiner(null, catalog.Environment.Logger);
        }
    }
}
