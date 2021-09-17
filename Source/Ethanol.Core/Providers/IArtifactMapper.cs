namespace Ethanol.Providers
{
    public interface IArtifactMapper<TRawRecord,TArtifact>
    { 
        public TArtifact Map(TRawRecord src);
    }
}