namespace Ethanol.Catalogs
{
    /// <summary>
    /// Represents the catalog of data loaders within the Ethanol environment.
    /// </summary>
    public class DataLoaderCatalog
    {
        // The environment to which this data loader catalog belongs.
        private EthanolEnvironment _ethanolEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLoaderCatalog"/> class with the specified environment.
        /// </summary>
        /// <param name="ethanolEnvironment">The Ethanol environment to associate with this catalog.</param>
        public DataLoaderCatalog(EthanolEnvironment ethanolEnvironment)
        {
            this._ethanolEnvironment = ethanolEnvironment;
        }
    }

}
