using Ethanol.Catalogs;
using Ethanol.DataObjects;
using System.IO;
using System.Net;

namespace Ethanol.ContextBuilder.Writers
{
    public static class JsonContextWriterCatalogEntry
    {
        public static ContextWriter<HostContext> GetJsonFileWriter(this ContextWriterCatalog catalog, TextWriter writer)
        {
            return JsonTargetHostContextWriter.CreateFileWriter(writer, catalog.Environment.Logger);
        }
        public static ContextWriter<HostContext> GetJsonTcpWriter(this ContextWriterCatalog catalog, IPEndPoint sendto)
        {
            return JsonTargetHostContextWriter.CreateTcpWriter(sendto, catalog.Environment.Logger);
        }
    }
}
