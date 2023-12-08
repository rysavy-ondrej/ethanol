using Cysharp.Text;
using Ethanol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;

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
        // >>>>>>>>>
        // CONFIGURE
        // >>>>>>>>>
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

            services.AddSingleton<EthanolEnvironment>();

        });

        // >>>>>>
        // BUILD
        // >>>>>>
        var app = builder.Build();

        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        LogManager.SetLoggerFactory(loggerFactory, "Global");

        app.AddCommands<ContextBuilderCommand>();
        app.AddCommands<ContextProviderCommand>();
        app.AddCommands<MalwareSonarCommands>();
        app.AddCommands<TagsHelperCommands>();

        // >>>
        // RUN
        // >>>
        try
        {
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.LogCritical(ex, $"ERROR: {ex.Message}");
        }
    }
}