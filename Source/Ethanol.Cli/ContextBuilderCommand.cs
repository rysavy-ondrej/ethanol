using Ethanol;
using Ethanol.Catalogs;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Pipeline;
using Ethanol.ContextBuilder.Polishers;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;

/// <summary>
/// Represents a command within a console application for running the context builder process.
/// </summary>
/// <remarks>
/// This class is responsible for handling the 'run-builder' command in the console application. 
/// It utilizes the EthanolEnvironment and ILogger to configure and log the execution of the context 
/// builder process. The command processes input options and starts the application based on the 
/// provided configuration file and optional progress reporting.
/// </remarks>
internal class ContextBuilderCommand : ConsoleAppBase
{
    private readonly ILogger _logger;
    private readonly EthanolEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextBuilderCommand"/> class.
    /// </summary>
    /// <param name="logger">Logger for logging information and errors.</param>
    /// <param name="environment">The environment configuration and settings for the application.</param>
    public ContextBuilderCommand(ILogger<ContextBuilderCommand> logger, EthanolEnvironment environment)
    {
        this._logger = logger;
        this._environment = environment;
    }

    /// <summary>
    /// Asynchronous command for running the builder process with specified options.
    /// </summary>
    /// <param name="configurationFile">The path to the configuration file used for processing setup.</param>
    /// <param name="progressReport">A boolean flag to enable or disable progress reporting during processing.</param>
    /// <returns>A task that represents the asynchronous operation of the builder process.</returns>
    /// <remarks>
    /// The command reads the specified configuration file to configure the processing environment 
    /// and can optionally report progress if enabled. The process involves setting up data flows, 
    /// context building, and other operations as defined in the configuration.
    /// </remarks>
    [Command("run-builder", "Starts the application.")]
    public async Task RunBuilderCommand(

    [Option("c", "The path to the configuration file used for processing setup.s")]
                string configurationFile,
    [Option("p", "Enable or disable progress reporting during processing.")]
                bool progressReport = true
    )
    {
        // 1.load configuration:
        var configurationFilePath = Path.GetFullPath(configurationFile);
        _logger?.LogInformation($"Running context builder with configuration file: '{configurationFilePath}'");
        var contextBuilderConfiguration = ContextBuilderConfiguration.LoadFromFile(configurationFilePath);
        // 2.create modules: 
        var readers = CreateInputReaders(contextBuilderConfiguration.Input, _environment);
        var writers = CreateOutputWriters(contextBuilderConfiguration.Output, _environment);
        var enricher = CreateEnricher(contextBuilderConfiguration.Enrichers, _environment);
        var polisher = _environment.ContextTransform.GetContextPolisher();
        var filter = GetFilter(contextBuilderConfiguration.Builder);
        var windowSpan = GetWindowSpan(contextBuilderConfiguration.Builder);
        // 3.set-up pipeline:
        var modules = new EthanolContextBuilder.BuilderModules(readers, writers, enricher, polisher);
        // 4.execute:
        await EthanolContextBuilder.Run(modules, windowSpan, contextBuilderConfiguration.Builder.FlowOrderingBufferSize, filter, progressReport ? Observer.Create<EthanolContextBuilder.BuilderStatistics>(OnProgressUpdate) : null);
    }

    EthanolContextBuilder.BuilderStatistics _lastReport;
    private void OnProgressUpdate(EthanolContextBuilder.BuilderStatistics report)
    {
        _logger.LogInformation($"{report}.");
        _lastReport = report;
    }

    private TimeSpan GetWindowSpan(ContextBuilderConfiguration.ContextBuilder builder)
    {
        return TimeSpan.TryParse(builder.WindowSize, out var span) ? span : throw new ArgumentException("Invalid WindowSize specified in configuration file.");
    }

    private IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> CreateEnricher(ContextBuilderConfiguration.Enrichers enricher, EthanolEnvironment environment)
    {
        if (enricher?.Netify != null && enricher.Netify.Postgres != null)
        {
            return environment.ContextTransform.GetNetifyPostgresEnricher(enricher.Netify.Postgres.GetConnectionString(), enricher.Netify.Postgres.TableName);
        }
        return environment.ContextTransform.GetVoidEnricher();
    }

    private static HostBasedFilter GetFilter(ContextBuilderConfiguration.ContextBuilder builder)
    {
        var prefixes = builder.Networks.Select(s => IPAddressPrefix.TryParse(s, out var prefix) ? prefix : null).Where(x => x != null).ToArray();
        var filter = (prefixes.Length != 0) ? new HostBasedFilter(prefixes) : new HostBasedFilter();
        return filter;
    }

    private static IDataReader<IpFlow>[] CreateInputReaders(ContextBuilderConfiguration.Input inputConfiguration, EthanolEnvironment environment)
    {
        var inputReaders = new List<IDataReader<IpFlow>>();
        // build the pipeline based on the configuration:
        if (inputConfiguration?.Stdin != null
        && inputConfiguration.Stdin.Format == "flowmon-json")
        {
            inputReaders.Add(environment.FlowReader.GetFlowmonFileReader(Console.In));
        }
        if (inputConfiguration?.Tcp != null
        && inputConfiguration.Tcp.Format == "flowmon-json")
        {
            var ip = inputConfiguration.Tcp.Listen;
            var port = inputConfiguration.Tcp.Port;
            var listenAt = IPEndPoint.TryParse($"{ip}:{port}", out var ep) ? ep : new IPEndPoint(IPAddress.Loopback, 8234);
            inputReaders.Add(environment.FlowReader.GetFlowmonTcpReader(listenAt));
        }
        return inputReaders.ToArray();
    }
    private static ContextWriter<ObservableEvent<IpTargetHostContext>>[] CreateOutputWriters(ContextBuilderConfiguration.Output outputConfiguration, EthanolEnvironment environment)
    {
        var outputWriters = new List<ContextWriter<ObservableEvent<IpTargetHostContext>>>();
        if (outputConfiguration?.Stdout != null)
        {
            if (outputConfiguration.Stdout.Format == "json")
            {
                outputWriters.Add(environment.ContextWriter.GetJsonFileWriter(Console.Out));
            }
            if (outputConfiguration.Stdout.Format == "yaml")
            {
                throw new NotImplementedException();
            }
        }
        if (outputConfiguration?.Postgres != null)
        {
            var connection = new NpgsqlConnection(outputConfiguration.Postgres.GetConnectionString());
            var tableName = outputConfiguration.Postgres.TableName;
            connection.Open();
            outputWriters.Add(environment.ContextWriter.GetPostgresWriter(connection, tableName));
        }
        return outputWriters.ToArray();
    }
}
