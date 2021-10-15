using Ethanol.Catalogs;

namespace Ethanol
{
    public class EthanolEnvironment
    {
        private readonly DataLoaderCatalog _dataLoaderCatalog;
        private readonly ContextBuilderCatalog _contextBuilderCatalog;

        public EthanolEnvironment()
        {
            _dataLoaderCatalog = new DataLoaderCatalog(this);
            _contextBuilderCatalog = new ContextBuilderCatalog(this);
        }

        public DataLoaderCatalog DataLoader => _dataLoaderCatalog;
        public ContextBuilderCatalog ContextBuilder => _contextBuilderCatalog;
    }
}
