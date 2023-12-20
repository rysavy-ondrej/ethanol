using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Readers;
using System.IO;
using System.Net;

namespace Ethanol.Catalogs
{
    /// <summary>
    /// Represents a catalog entry for IPFIXCOL readers.
    /// </summary>
    public static class IpfixcolReaderCatalogEntry
    {
        /// <summary>
        /// Gets an IPFIXCOL file reader.
        /// </summary>
        /// <param name="catalog">The flow reader catalog.</param>
        /// <param name="reader">The text reader.</param>
        /// <returns>An instance of <see cref="IDataReader{IpFlow}"/> for reading IPFIXCOL files.</returns>
        public static IDataReader<IpFlow> GetIpfixcolFileReader(this FlowReaderCatalog catalog, TextReader reader)
        {
            return IpfixcolJsonReader.CreateFileReader(reader, catalog.Environment.Logger);
        }

        /// <summary>
        /// Gets an IPFIXCOL TCP reader.
        /// </summary>
        /// <param name="catalog">The flow reader catalog.</param>
        /// <param name="listenAt">The IP endpoint to listen at.</param>
        /// <returns>An instance of <see cref="IDataReader{IpFlow}"/> for reading IPFIXCOL over TCP.</returns>
        public static IDataReader<IpFlow> GetIpfixcolTcpReader(this FlowReaderCatalog catalog, IPEndPoint listenAt)
        {
            return IpfixcolJsonReader.CreateTcpReader(listenAt, catalog.Environment.Logger);
        }
    }
}
