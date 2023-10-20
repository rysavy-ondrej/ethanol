using Ethanol.Catalogs;

namespace Ethanol
{
    /// <summary>
    /// Represents the core environment configuration for Ethanol.
    /// </summary>
    public class EthanolEnvironment
    {
        // Data loader catalog to manage the data loading components.
        private readonly DataLoaderCatalog _dataLoaderCatalog;

        // Context builder catalog to manage the context-building components.
        private readonly ContextBuilderCatalog _contextBuilderCatalog;

        /// <summary>
        /// Initializes a new instance of the <see cref="EthanolEnvironment"/> class.
        /// </summary>
        public EthanolEnvironment()
        {
            _dataLoaderCatalog = new DataLoaderCatalog(this);
            _contextBuilderCatalog = new ContextBuilderCatalog(this);
        }

        /// <summary>
        /// Gets the data loader catalog associated with this environment.
        /// </summary>
        public DataLoaderCatalog DataLoader => _dataLoaderCatalog;

        /// <summary>
        /// Gets the context builder catalog associated with this environment.
        /// </summary>
        public ContextBuilderCatalog ContextBuilder => _contextBuilderCatalog;
    }

}
