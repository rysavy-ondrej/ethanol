using Ethanol.ContextBuilder.Context;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Factory class supporting to instantiating flow readers.
    /// </summary>
    public static class FlowReaderFactory
    {
        /// <summary>
        /// Creates flow reader according the given <paramref name="moduleSpecification"/>.
        /// </summary>
        /// <param name="moduleSpecification">The reader modules specification.</param>
        /// <returns>Reader module or null if a reader of the given name does not exist.</returns>
        public static InputDataReader<IpfixRecord> GetReader(ModuleSpecification moduleSpecification)
        {
            switch (moduleSpecification?.Name)
            {
                case nameof(FlowmonJsonReader): return FlowmonJsonReader.Create(moduleSpecification.Parameters);
            }
            return null;
        }
    }
}
