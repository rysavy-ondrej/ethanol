using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Pipeline;
using System;

namespace Ethanol.ContextBuilder.Builders
{
    public static class IpHostContextBuilderCatalogEntry
    {
        public static IpHostContextBuilder GetIpHostContextBuilder(this ContextBuilderCatalog catalog, TimeSpan windowSize, TimeSpan windowHop, HostBasedFilter filter)
        {
            return new IpHostContextBuilder(windowSize, windowHop, filter);
        }
    }
}
