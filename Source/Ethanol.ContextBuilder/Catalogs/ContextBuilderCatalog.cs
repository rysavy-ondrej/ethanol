namespace Ethanol.Catalogs
{

    /// <summary>
    /// Represents the catalog of context builders within the Ethanol environment.
    /// </summary>
    public class ContextBuilderCatalog
    {
        // The environment to which this context builder catalog belongs.
        private EthanolEnvironment ethanolEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextBuilderCatalog"/> class with the specified environment.
        /// </summary>
        /// <param name="ethanolEnvironment">The Ethanol environment to associate with this catalog.</param>
        public ContextBuilderCatalog(EthanolEnvironment ethanolEnvironment)
        {
            this.ethanolEnvironment = ethanolEnvironment;
        }

        public EthanolEnvironment Environment => ethanolEnvironment;
    }
}
