using Ethanol.ContextBuilder.Refiners;
using Ethanol.DataObjects;

namespace Ethanol.Catalogs
{
    public static class ContextRefinerCatalogEntry
    {
        /// <summary>
        /// Gets the context polisher for IpHostContextWithTags.
        /// </summary>
        /// <param name="catalog">The ContextTransformCatalog.</param>
        /// <returns>The context polisher.</returns>
        public static IRefiner<TimeRange<IpHostContextWithTags>, HostContext> GetContextRefiner(this ContextBuilderCatalog catalog)
        {
            return new IpHostContextRefiner(null, catalog.Environment.Logger);
        }
    }
}
