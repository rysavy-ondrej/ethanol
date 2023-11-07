namespace Ethanol.Catalogs
{
    public class ContextWriterCatalog
    {
        // The environment to which this data loader catalog belongs.
        private EthanolEnvironment _ethanolEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowReaderCatalog"/> class with the specified environment.
        /// </summary>
        /// <param name="ethanolEnvironment">The Ethanol environment to associate with this catalog.</param>
        public ContextWriterCatalog(EthanolEnvironment ethanolEnvironment)
        {
            this._ethanolEnvironment = ethanolEnvironment;
        }

        public EthanolEnvironment Environment => _ethanolEnvironment; 
    }

}
