using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Readers;
using System.IO;
using System.Net;

namespace Ethanol.Catalogs
{
    /// <summary>
    /// Provides extension methods for the <see cref="FlowReaderCatalog"/> class to create Flowmon readers.
    /// </summary>
    public static class FlowmonReaderCatalogEntry
    {
        /// <summary>
        /// Creates a Flowmon file reader using the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="catalog">The <see cref="FlowReaderCatalog"/> instance.</param>
        /// <param name="reader">The <see cref="TextReader"/> to read the Flowmon file.</param>
        /// <returns>An instance of <see cref="IDataReader{IpFlow}"/> for reading Flowmon data from a file.</returns>
        /// <summary>
        /// Gets a Flowmon file reader from the catalog.
        /// </summary>
        /// <param name="catalog">The flow reader catalog.</param>
        /// <param name="reader">The text reader.</param>
        /// <param name="filePath">The file path or null if stdin is used.</param>
        /// <returns>The Flowmon file reader.</returns>
        public static IDataReader<IpFlow> GetFlowmonFileReader(this FlowReaderCatalog catalog, TextReader reader, string? filePath)
        {
            return FlowmonJsonReader.CreateFileReader(reader, filePath, catalog.Environment.Logger);
        }

        /// <summary>
        /// Creates a Flowmon TCP reader using the specified <see cref="IPEndPoint"/>.
        /// </summary>
        /// <param name="catalog">The <see cref="FlowReaderCatalog"/> instance.</param>
        /// <param name="listenAt">The <see cref="IPEndPoint"/> to listen for Flowmon TCP data.</param>
        /// <returns>An instance of <see cref="IDataReader{IpFlow}"/> for reading Flowmon data over TCP.</returns>
        public static IDataReader<IpFlow> GetFlowmonTcpReader(this FlowReaderCatalog catalog, IPEndPoint listenAt)
        {
            return FlowmonJsonReader.CreateTcpReader(listenAt, catalog.Environment.Logger);
        }
    }
}
