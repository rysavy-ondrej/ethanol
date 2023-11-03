using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ethanol.Cli
{
    internal class ContextBuilder : ConsoleAppBase
    {
        private readonly ILogger _logger;
        private readonly IConfigurationBuilder configurationBuilder;

        public ContextBuilder(ILogger<ContextBuilder> logger, IConfigurationBuilder configurationBuilder)
        {
            this._logger = logger;
            this.configurationBuilder = configurationBuilder;
        }

        [Command("run-builder", "Starts the application.")]
        public async Task RunBuilderCommand(

        [Option("c", "The configuration file used to configure the processing.")]
                string configurationFile
        )
        {
            _logger.LogInformation($"Running context builder with configuration file: '{configurationFile}'");
            var configuration = configurationBuilder.AddJsonFile(configurationFile).Build();
        }
    }
}