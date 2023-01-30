using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Readers
{
    public static class FlowReaderFactory
    {
        public static InputDataReader<IpfixRecord> GetReader(ModuleSpecification moduleSpecification)
        {
            switch (moduleSpecification?.Name)
            {
                case nameof(FlowmonJsonReader): return FlowmonJsonReader.Create(moduleSpecification.Attributes);
            }
            return null;
        }
    }
}
