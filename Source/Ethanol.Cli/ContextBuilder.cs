using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ConfigurationSubstitution;
namespace Ethanol.Cli
{
    internal class ContextBuilder : ConsoleAppBase
    {
        private readonly ILogger _logger;        

        public ContextBuilder(ILogger<ContextBuilder> logger)
        {
            this._logger = logger;
        }

        [Command("run-builder", "Starts the application.")]
        public async Task RunBuilderCommand(

        [Option("c", "The configuration file used to configure the processing.")]
                string configurationFile
        )
        {
            _logger.LogInformation($"Running context builder with configuration file: '{configurationFile}'");
            var configurationBuilder = new ConfigurationBuilder();
            var configuration = configurationBuilder.AddJsonFile(configurationFile).AddEnvironmentVariables().EnableSubstitutions("${", "}", UnresolvedVariableBehaviour.IgnorePattern).Build();

            Console.WriteLine($"{configuration.GetValue<string>("window-size")}");


        }
    }
}