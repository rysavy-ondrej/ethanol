using Microsoft.Extensions.Logging;

namespace Ethanol.Cli
{
    internal class ContextProvider : ConsoleAppBase
    {
        private readonly ILogger _logger;

        public ContextProvider(ILogger<ContextProvider> logger)
        {
            this._logger = logger;
        }
        [Command("start-service", "Starts the API for accesing the context objects.")]
        public async Task RunBuilderCommand(
            [Option("c", "The configuration file used to configure the service.")] string configurationFile
        )
        {
            var configurationFilePath = Path.GetFullPath(configurationFile);
            _logger?.LogInformation($"Running context provider with configuration file: '{configurationFilePath}'");
        
            var configuration = EthanolConfiguration.LoadFromFile(configurationFilePath);

            var service = new Ethanol.ContextProvider.ProviderService();
            await service.RunService(configuration, _logger);
        }
    }
}