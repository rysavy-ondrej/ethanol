﻿using Ethanol;
using Ethanol.DataObjects;
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
[Command("builder", "Context builder related commands.")]
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
    [Command("run", "Runs the context builder command according to the configuration file.")]
    public async Task RunBuilderCommand(

    [Option("c", "The path to the configuration file used for processing setup.")]
                string configurationFile,
    [Option("p", "Enable or disable progress reporting during processing.")]
                bool progressReport = true
    )
    {
        try
        {
            // 1.load configuration:
            var configurationFilePath = Path.GetFullPath(configurationFile);
            _logger?.LogInformation($"Running context builder with configuration file: '{configurationFilePath}'");
            var contextBuilderConfiguration = ContextBuilderConfiguration.LoadFromFile(configurationFilePath);

            // 2.create modules: 
            var readers = CreateInputReaders(contextBuilderConfiguration.Input, _environment);

            if (readers.Length == 0) throw new ArgumentException("No 'input' configuration specified in configuration file.");

            _logger?.LogInformation($"Reader(s): {String.Join(',', readers.Select(w => w.ToString()))}");

            var writers = CreateOutputWriters(contextBuilderConfiguration.Output, _environment);

            if (writers.Length == 0) throw new ArgumentException("No 'output' configuration specified in configuration file.");

            _logger?.LogInformation($"Writer(s): {String.Join(',', writers.Select(w => w.ToString()))}");

            var enricher = CreateEnricher(contextBuilderConfiguration.Enrichers, _environment);
            var polisher = _environment.ContextTransform.GetContextPolisher();
            var filter = GetFilter(contextBuilderConfiguration.Builder);
            var windowSpan = GetWindowSpan(contextBuilderConfiguration.Builder);
            
            // 3.set-up pipeline:
            var modules = new EthanolContextBuilder.BuilderModules(readers, writers, enricher, polisher);
            
            // 4.execute:
            var stats = await EthanolContextBuilder.Run(modules, windowSpan, contextBuilderConfiguration.Builder?.FlowOrderingBufferSize ?? 0, filter, progressReport ? Observer.Create<EthanolContextBuilder.BuilderStatistics>(OnProgressUpdate) : null);
            _logger?.LogInformation($"Builder finished: {stats.ToString()}");
        }
        catch(Exception ex)
        {
            _logger.LogCritical(ex, $"Cannot run builder: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes the context builder that reads data on stdin and produces contexts to stdout.
    /// </summary>
    /// <param name="windowSpan">Configuration of the window size for the builder. Default is 5 minutes.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Command("exec", "Executes the context builder that reads data on stdin and produces contexts to stdout.")]
    public async Task ExecBuilderCommand(
        [Option("w", "Configuration of the window size for the builder. Default is 5 minutes.")]
        string windowSpan="00:05:00",
        [Option("f", "Input format. Default is flowmon-json.")]
        string inputFormat="flowmon-json",
        [Option("i", "Input file path. If not specified, 'stdin' is used.")]
        string? inputFilePath=null       
        )
        {
        try
        {
            var readers = new[] { GetStdinReaderFormat(inputFormat, inputFilePath) };
            var writers = new[] { _environment.ContextWriter.GetJsonFileWriter(Console.Out, null) };
            var enricher = _environment.ContextTransform.GetVoidEnricher(); 
            var polisher = _environment.ContextTransform.GetContextPolisher();
            var filter = new HostBasedFilter();
            var windowTimeSpan = TimeSpan.Parse(windowSpan);
            var modules = new EthanolContextBuilder.BuilderModules(readers, writers, enricher, polisher);
            var stats = await EthanolContextBuilder.Run(modules, windowTimeSpan, 8, filter, null);
            _logger?.LogInformation($"Builder finished: {stats.ToString()}");
        }
        catch(Exception ex)
        {
            _logger.LogCritical(ex, $"Cannot execute builder: {ex.Message}");
        }
    }

    private IDataReader<IpFlow> GetStdinReaderFormat(string inputFormat, string? filePath = null)
    {
        if (filePath != null && !File.Exists(filePath)) throw new ArgumentException($"Input file '{filePath}' does not exist.");

        var textReader = filePath != null ? new StreamReader(filePath) : Console.In;
        switch (inputFormat)
        {
            case "flowmon-json":
                return _environment.FlowReader.GetFlowmonFileReader(textReader, filePath);
            case "ipfixcol-json":
                return _environment.FlowReader.GetIpfixcolFileReader(textReader, filePath);
            default:
                throw new ArgumentException($"Invalid or missing stdin format specified '{inputFormat}'.");
        }
    }

    private void OnProgressUpdate(EthanolContextBuilder.BuilderStatistics report)
    {
        _logger.LogInformation($"{report}.");
    }

    private TimeSpan GetWindowSpan(ContextBuilderConfiguration.ContextBuilder? builder)
    {
        if (builder == null) return TimeSpan.FromMinutes(5);

        return TimeSpan.TryParse(builder.WindowSize, out var span) ? span : throw new ArgumentException("Invalid WindowSize specified in configuration file.");
    }

    private IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> CreateEnricher(ContextBuilderConfiguration.Enrichers? enricher, EthanolEnvironment environment)
    {
        if (enricher==null)
        {
            return environment.ContextTransform.GetVoidEnricher();
        }

        if (enricher?.Netify != null && enricher.Netify.Postgres != null)
        {
            return environment.ContextTransform.GetNetifyPostgresEnricher(enricher.Netify.Postgres.GetConnectionString(), enricher.Netify.Postgres.TableName);
        }
    
        return environment.ContextTransform.GetVoidEnricher();
    }

    private static HostBasedFilter GetFilter(ContextBuilderConfiguration.ContextBuilder? builder)
    {
        if (builder?.Networks == null) return new HostBasedFilter();
        else
        {
            var prefixes = builder.Networks.Select(s => IPAddressPrefix.TryParse(s, out var prefix) ? prefix : null).Where(x => x != null).ToArray();
            var filter = (prefixes.Length != 0) ? new HostBasedFilter(prefixes) : new HostBasedFilter();
            return filter;
        }
    }

    private static IDataReader<IpFlow>[] CreateInputReaders(IEnumerable<ContextBuilderConfiguration.Input>? inputConfigurations, EthanolEnvironment environment)
    {
        if (inputConfigurations == null)
        {
            throw new ArgumentException("No input configuration specified in configuration file.");
        }

        var inputReaders = inputConfigurations.SelectMany(c => CreateInputReader(c, environment)).ToArray();
        return inputReaders;
    }
    private static IEnumerable<IDataReader<IpFlow>> CreateInputReader(ContextBuilderConfiguration.Input inputConfiguration, EthanolEnvironment environment)
    {
        // build the pipeline based on the configuration:
        if (inputConfiguration?.Stdin != null)
        {
            switch(inputConfiguration.Stdin.Format?.ToLowerInvariant() ?? string.Empty)
            {
                case "flowmon-json":
                    yield return environment.FlowReader.GetFlowmonFileReader(Console.In, null);
                    break;
                case "ipfixcol-json":
                    yield return environment.FlowReader.GetIpfixcolFileReader(Console.In, null);
                    break;
                default:
                    throw new ArgumentException($"Invalid or missing stdin format '{inputConfiguration.Stdin.Format}' specified in configuration file.");
            }
        }
        if (inputConfiguration?.Tcp != null)        
        {
            var ip = inputConfiguration.Tcp.Listen;
            var port = inputConfiguration.Tcp.Port;
            var listenAt = IPEndPoint.TryParse($"{ip}:{port}", out var ep) ? ep : new IPEndPoint(IPAddress.Loopback, 8234);

            switch(inputConfiguration.Tcp.Format?.ToLowerInvariant() ?? string.Empty)
            {
                case "flowmon-json":
                    yield return environment.FlowReader.GetFlowmonTcpReader(listenAt);
                    break;
                case "ipfixcol-json":
                    yield return environment.FlowReader.GetIpfixcolTcpReader(listenAt);
                    break;
                default:
                    throw new ArgumentException($"Invalid or missing tcp format specified '{inputConfiguration.Tcp.Format}' in configuration file.");
            }
        }
    }
    private static ContextWriter<HostContext>[] CreateOutputWriters(IEnumerable<ContextBuilderConfiguration.Output>? outputConfiguration, EthanolEnvironment environment)
    {
        if (outputConfiguration == null)
        {
            throw new ArgumentException("No output configuration specified in configuration file.");
        }
        var outputWriters = outputConfiguration.SelectMany(c => CreateOutputWriter(c, environment)).ToArray();
        return outputWriters;
    }
     private static IEnumerable<ContextWriter<HostContext>> CreateOutputWriter(ContextBuilderConfiguration.Output outputConfiguration, EthanolEnvironment environment)
     {
        if (outputConfiguration?.Stdout != null)
        {
            if (outputConfiguration.Stdout.Format == "json")
            {
                yield return environment.ContextWriter.GetJsonFileWriter(Console.Out, null);
            }
            if (outputConfiguration.Stdout.Format == "yaml")
            {
                throw new NotImplementedException("Output format 'yaml' is currently not supported.");
            }
        }
        if (outputConfiguration?.Postgres != null)
        {
            var connection = new NpgsqlConnection(outputConfiguration.Postgres.GetConnectionString());
            var tableName = outputConfiguration.Postgres.TableName;
            connection.Open();
            yield return environment.ContextWriter.GetPostgresWriter(connection, tableName);
        }
    }
}
