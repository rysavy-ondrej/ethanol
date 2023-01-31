namespace Ethanol.ContextBuilder.Writers
{
    /// <summary>
    /// Factory class for creating writer modules.
    /// </summary>
    public static class WriterFactory
    {
        /// <summary>
        /// Creates writer object according the given <paramref name="moduleSpecification"/>.
        /// </summary>
        /// <param name="moduleSpecification">The writer modules specification.</param>
        /// <returns>Writer module or null if a writer of the given name does not exist.</returns>
        internal static WriterModule<object> GetWriter(ModuleSpecification outputModule)
        {
            switch(outputModule?.Name)
            {
                case nameof(YamlDataWriter): return YamlDataWriter.Create(outputModule.Parameters);
                case nameof(JsonDataWriter): return JsonDataWriter.Create(outputModule.Parameters);
            }
            return null;
        }
    }
}
