using Ethanol;
using Microsoft.Extensions.Logging;

[Command("service", "The REST API service for accesing the context objects.")]
internal class ContextProviderCommand : ConsoleAppBase
{
    private readonly ILogger _logger;
    private readonly EthanolEnvironment _environment;

    public ContextProviderCommand(ILogger<ContextProviderCommand> logger, EthanolEnvironment environment)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    [Command("start", "Starts the API for accesing the context objects.")]
    public void RunBuilderCommand(
        [Option("c", "The configuration file used to configure the service.")] string configurationFile
    )
    {
        var configurationFilePath = Path.GetFullPath(configurationFile);
        _logger.LogInformation($"Running context provider with configuration file: '{configurationFilePath}'");

        var configuration = EthanolConfiguration.LoadFromFile(configurationFilePath);

        var service = new Ethanol.ContextProvider.ProviderService();
        service.RunService(configuration, _logger!);
    }
}
