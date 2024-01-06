using ConsoleAppFramework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ZLogger;
using Cysharp.Text;

/// <summary>
/// The program class containing the main entry point.
/// </summary>
public class Program : ConsoleAppBase
{
    /// <summary>
    /// Entry point to the console application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public async static Task Main(string[] args)
    {
        var builder = ConsoleApp.CreateBuilder(args);


        builder.ConfigureServices((ctx, services) =>
        {
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddZLoggerConsole(options =>
                {
                    options.PrefixFormatter = (writer, info) => ZString.Utf8Format(writer, "[{0}][{1}] ", info.LogLevel, info.Timestamp.DateTime.ToLocalTime());
                }, outputToErrorStream: true);
#if DEBUG
                logging.SetMinimumLevel(LogLevel.Trace);
#else
                logging.SetMinimumLevel(LogLevel.Information);
#endif
        
            });
        });

        // BUILD
        // >>>>>>
        var app = builder.Build();
        app.AddSubCommands<BuilderStressTestCommand>();

        


        // >>>
        // RUN
        // >>>
        try
        {
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogCritical(ex, $"ERROR: {ex.Message}");
        }
    }
}

[Command("builder", "The collection of builder test commands.")]
internal class BuilderStressTestCommand : ConsoleAppBase
{
    private readonly ILogger _logger;

    public BuilderStressTestCommand(ILogger<BuilderStressTestCommand> logger)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Command("replay-flows", "Replays the given JSON flow file accroding to the given arguments.")]
    public void ReplayFlows(
        [Option("i", "The flow file to replay.")] string flowFile,
        [Option("f", "The flow file to replay.")] string flowFormat,
        [Option("t", "Tcp target host.")] string tcpEndpoint,
        [Option("s", "Speed of replay.")] double speed,
        [Option("r", "Randomize addresses.")] bool randomAddresses = false,
        [Option("e", "Command timeout. Default is infinite.")] TimeSpan? timeout = null
    )
    {
        var flowFilePath = Path.GetFullPath(flowFile);
        _logger.LogInformation($"Running builder stress test with flow file: '{flowFilePath}'");
        var flows = FlowFile.LoadFromFile(flowFilePath, getFormatter(flowFormat));

        // perform flow generation and sending to TCP ...


    }

    private FlowJsonFormatManipulator getFormatter(string? flowFormat)
    {
        if (flowFormat == null) throw new ArgumentNullException(nameof(flowFormat), "The flow format cannot be null.");
        switch(flowFormat)
        {
            case "ipficol-json":
                return new IpfixcolJsonFormatManipulator();
            case "flowmon-json":
                return new FlowmonJsonFormatManipulator();
            default:
                throw new ArgumentException($"Unknown flow format: '{flowFormat}'.");
        }
    }
}


