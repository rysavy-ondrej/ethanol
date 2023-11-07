using Ethanol.Catalogs;
using Ethanol.ContextBuilder;
using Microsoft.Extensions.Logging;

namespace Ethanol
{
    /// <summary>
    /// Represents the core environment configuration for Ethanol.
    /// </summary>
    public class EthanolEnvironment
    {
        // Data loader catalog to manage the data loading components.
        private readonly FlowReaderCatalog _flowReaderCatalog;

        // Context builder catalog to manage the context-building components.
        private readonly ContextBuilderCatalog _contextBuilderCatalog;

        private readonly ContextEnricherCatalog _contextEnricherCatalog;

        private readonly ContextWriterCatalog _contextWriterCatalog;
        private static ILogger __logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EthanolEnvironment"/> class.
        /// </summary>
        public EthanolEnvironment(ILogger logger)
        {
            _flowReaderCatalog = new FlowReaderCatalog(this);
            _contextBuilderCatalog = new ContextBuilderCatalog(this);
            _contextEnricherCatalog = new ContextEnricherCatalog(this);
            _contextWriterCatalog = new ContextWriterCatalog(this);
            __logger = logger;
        }

        /// <summary>
        /// Gets the data loader catalog associated with this environment.
        /// </summary>
        public FlowReaderCatalog FlowReader => _flowReaderCatalog;

        /// <summary>
        /// Gets the context builder catalog associated with this environment.
        /// </summary>
        public ContextBuilderCatalog ContextBuilder => _contextBuilderCatalog;

        public ContextEnricherCatalog ContextEnricher => _contextEnricherCatalog;

        public ContextWriterCatalog ContextWriter => _contextWriterCatalog;

        public static ILogger DefaultLogger => __logger;

        public ILogger Logger => __logger;
    }

}
