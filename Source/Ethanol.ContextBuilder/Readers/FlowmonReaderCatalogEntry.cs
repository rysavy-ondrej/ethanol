using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Readers;
using System.IO;
using System.Net;

namespace Ethanol.Catalogs
{
    public static class FlowmonReaderCatalogEntry
    {
        public static IFlowReader<IpFlow> GetFlowmonFileReader(this FlowReaderCatalog catalog, TextReader reader)
        {
            return Flowmonexp5JsonReader.CreateFileReader( reader, catalog.Environment.Logger);
        }
        public static IFlowReader<IpFlow> GetFlowmonTcpReader(this FlowReaderCatalog catalog, IPEndPoint listenAt)
        {
            return Flowmonexp5JsonReader.CreateTcpReader(listenAt, catalog.Environment.Logger);
        }
    }
}
