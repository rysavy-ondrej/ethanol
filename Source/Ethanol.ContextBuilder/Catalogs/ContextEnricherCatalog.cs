namespace Ethanol
{
    public class ContextEnricherCatalog
    {
        private EthanolEnvironment ethanolEnvironment;

        public ContextEnricherCatalog(EthanolEnvironment ethanolEnvironment)
        {
            this.ethanolEnvironment = ethanolEnvironment;
        }

        public EthanolEnvironment Environment => ethanolEnvironment;
    }
}