namespace Ethanol
{
    public class ContextTransformCatalog
    {
        private EthanolEnvironment ethanolEnvironment;

        public ContextTransformCatalog(EthanolEnvironment ethanolEnvironment)
        {
            this.ethanolEnvironment = ethanolEnvironment;
        }

        public EthanolEnvironment Environment => ethanolEnvironment;
    }
}