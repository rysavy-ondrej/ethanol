using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Polishers;
using System.IO;
using System.Net;

namespace Ethanol.ContextBuilder.Writers
{
    public static class JsonContextWriterCatalogEntry
    {
        public static ContextWriter<ObservableEvent<IpTargetHostContext>> GetJsonFileWriter(this ContextWriterCatalog catalog, TextWriter writer)
        {
            return JsonTargetHostContextWriter.CreateFileWriter(writer, catalog.Environment.Logger);
        }
        public static ContextWriter<ObservableEvent<IpTargetHostContext>> GetJsonTcpWriter(this ContextWriterCatalog catalog, IPEndPoint sendto)
        {
            return JsonTargetHostContextWriter.CreateTcpWriter(sendto, catalog.Environment.Logger);
        }
    }
}
