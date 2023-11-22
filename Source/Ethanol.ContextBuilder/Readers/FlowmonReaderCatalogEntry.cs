﻿using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Readers;
using System.IO;
using System.Net;

namespace Ethanol.Catalogs
{
    public static class FlowmonReaderCatalogEntry
    {
        public static IDataReader<IpFlow> GetFlowmonFileReader(this FlowReaderCatalog catalog, TextReader reader)
        {
            return FlowmonJsonReader.CreateFileReader( reader, catalog.Environment.Logger);
        }
        public static IDataReader<IpFlow> GetFlowmonTcpReader(this FlowReaderCatalog catalog, IPEndPoint listenAt)
        {
            return FlowmonJsonReader.CreateTcpReader(listenAt, catalog.Environment.Logger);
        }
    }
}