﻿using ConsoleAppFramework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ZLogger;
using Cysharp.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Reactive.Linq;
using System.Text.Json;
using Npgsql;
using Ethanol.ContextBuilder.Helpers;
using Ethanol.DataObjects;
using Humanizer;
using Ethanol.ContextBuilder.Serialization;

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

// User ID=myUsername;Password=myPassword;Host=myServerAddress;Port=5432;Database=myDataBase;

    [Command("replay-tags", "Replays the given Tags file using the given arguments.")]
    public void ReplayTags(
        [Option("i", "The flow file to replay.")] string tagFile,
        [Option("d", "Database connection string. Example: 'User ID=postgres;password=postgres;Host=localhost;Port=1605;Database=ethanol'")] string connectionString,
        [Option("t", "Database tablename.")] string tableName, 
        [Option("p", "Address prefix to use for the tag data.")] string addressPrefix,
        [Option("s", "Average delay between records in seconds.")] double interRecordDelay = 0,
        [Option("c", "The number of tags to read from soure flow file.")] int tagCount = 10000,
        [Option("e", "Command timeout. Default is infinite.")] TimeSpan? timeout = null
    )
    {
        var flowFilePath = Path.GetFullPath(tagFile);
        
        var random = new Random();
        var sw = new Stopwatch();
        sw.Start();
        long sentTagsCount = 0;
        long lastSentTagsCount = 0;
        double lastMiliseconds = 0.0;
        void printProgress(long obj)
        {
            var actualFps = (sentTagsCount - lastSentTagsCount) / ((sw.ElapsedMilliseconds - lastMiliseconds) / 1000.0);
            lastSentTagsCount = sentTagsCount;
            lastMiliseconds = sw.ElapsedMilliseconds;     
            var averageFps = sentTagsCount / (sw.ElapsedMilliseconds / 1000.0);
            var elapsed = TimeSpan.FromSeconds(sw.ElapsedMilliseconds / 1000.0);
            var currentTimestamp = DateTimeOffset.Now;
            Console.Write($"now:{currentTimestamp},tags: {sentTagsCount}, elapsed: {elapsed.ToString(@"hh\:mm\:ss")}, speed: {actualFps:F2} fps, average: {averageFps:F2} fps.                    \r");
        }

        //var t = Observable.Interval(TimeSpan.FromMilliseconds(100)).ForEachAsync(printProgress, Context.CancellationToken);
        Console.WriteLine($"Start generating flows:");

        var ipPrefix = (IPAddressPrefix.TryParse(addressPrefix, out var adr) ? adr.Address.GetAddressBytes()[..(adr.PrefixLength/8)] : null) ?? throw new ArgumentException($"Invalid address prefix: '{addressPrefix}'.");

        _logger.LogInformation($"Running builder stress test with tag file: '{tagFile}'. Use random addresses from the prefix={ipPrefix}");


        var tags = JsonSampleFile.LoadFromFile(flowFilePath, new TagJsonFormatManipulator(ipPrefix), tagCount, _logger);

        _logger.LogInformation($"Connecting to database: '{connectionString}'...");         
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        _logger.LogInformation($"Connected.");
        LinkedList<TagObject> tagObjects = new LinkedList<TagObject>();
        var jsonOption = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true,
            Converters = { new DateTimeOffsetJsonConverter() }
        };
        while (Context.CancellationToken.IsCancellationRequested == false && sw.ElapsedMilliseconds < (timeout?.TotalMilliseconds ?? long.MaxValue))
        {
            string tag = String.Empty;
            try
            {
                tag = tags.GetNextSample();
                if (tag == null) continue;
                var tagObject = JsonSerializer.Deserialize<TagObject>(tag, jsonOption);
                if (tagObject == null) continue;

                tagObject.StartTime = DateTimeOffset.Now - TimeSpan.FromDays(1);
                tagObject.EndTime = DateTimeOffset.Now + TimeSpan.FromDays(1);
                tagObjects.AddLast(tagObject);
            }
            catch (JsonException e)
            {
               // _logger?.LogError(e, $"Error parsing tag: {tag}");
            }

            if (tagObjects.Count > 1000)
            {
                _logger?.LogInformation($"Inserting {tagObjects.Count} tags.");
                PostgresTagDataSource.BulkInsert(connection, tableName, tagObjects);
                tagObjects.Clear();
            }
            
            sentTagsCount++;
        }
        PostgresTagDataSource.BulkInsert(connection, tableName, tagObjects);
    }

    [Command("replay-flows", "Replays the given JSON flow file using the given arguments.")]
    public void ReplayFlows(
        [Option("i", "The flow file to replay.")] string flowFile,
        [Option("f", "The flow file to replay.")] string flowFormat,
        [Option("c", "The number of flows to read from soure flow file.")] int flowCount,
        [Option("t", "Tcp target host.")] string tcpEndpoint,
        [Option("r", "Randomize addresses")] bool randomizeAddresses = false,
        [Option("s", "Average delay between records in seconds.")] double interRecordDelay = 0,
        [Option("e", "Command timeout. Default is infinite.")] TimeSpan? timeout = null
    )
    {
        var flowFilePath = Path.GetFullPath(flowFile);
        _logger.LogInformation($"Running builder stress test with flow file: '{flowFilePath}'. Randomize address={randomizeAddresses}");
        var flows = JsonSampleFile.LoadFromFile(flowFilePath, getFormatter(flowFormat, randomizeAddresses), flowCount);

        var tcpIpEndpoint = IPEndPoint.Parse(tcpEndpoint);
        var random = new Random();
        var sw = new Stopwatch();
        sw.Start();
        long sentFlowCount = 0;
        long lastSentFlowCount = 0;
        double lastMiliseconds = 0.0;
        void printProgress(long obj)
        {
            var actualFps = (sentFlowCount - lastSentFlowCount) / ((sw.ElapsedMilliseconds - lastMiliseconds) / 1000.0);
            lastSentFlowCount = sentFlowCount;
            lastMiliseconds = sw.ElapsedMilliseconds;     
            var averageFps = sentFlowCount / (sw.ElapsedMilliseconds / 1000.0);
            var elapsed = TimeSpan.FromSeconds(sw.ElapsedMilliseconds / 1000.0);
            var currentTimestamp = DateTimeOffset.Now;

            Console.Write($"now:{currentTimestamp}, flows: {sentFlowCount}, elapsed: {elapsed.ToString(@"hh\:mm\:ss")}, speed: {actualFps:F2} fps, average: {averageFps:F2} fps.                    \r");
        }

        var t = Observable.Interval(TimeSpan.FromMilliseconds(100)).ForEachAsync(printProgress, Context.CancellationToken);


        Console.WriteLine($"Start generating flows:");

        var client = new TcpClient();
        client.Connect(tcpIpEndpoint);
        using var stream = client.GetStream();
        try
        {
            while (Context.CancellationToken.IsCancellationRequested == false && sw.ElapsedMilliseconds < (timeout?.TotalMilliseconds ?? long.MaxValue))
            {
                var flow = flows.GetNextSample();
                if (flow == null) continue;
                var flowBytes = Encoding.UTF8.GetBytes(flow);
                stream.Write(flowBytes, 0, flowBytes.Length);
                sentFlowCount++;
                if (interRecordDelay > 0)
                {
                    var delay = (int)(interRecordDelay * 1000);
                    if (delay > 10)
                    {
                        var waitTime = delay + random.Next(-delay / 10, delay / 10);
                        Thread.Sleep(waitTime);
                    }
                    else
                    {
                        Thread.Sleep(delay);
                    }
                }
            }
        }
        catch(SocketException ex)
        {
            _logger.LogError(ex, $"Socket exception: {ex.Message}");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Exception: {ex.Message}");
        }
        finally
        {
            client.Close();
            _logger.LogInformation($"Sent {sentFlowCount} flows in {sw.ElapsedMilliseconds} ms.");
        }
        
    }

    private JsonFormatManipulator getFormatter(string? flowFormat, bool randomizeAddresses = false)
    {
        if (flowFormat == null) throw new ArgumentNullException(nameof(flowFormat), "The flow format cannot be null.");
        switch(flowFormat)
        {
            case "ipficol-json":
                return new IpfixcolJsonFormatManipulator(randomizeAddresses);
            case "flowmon-json":
                return new FlowmonJsonFormatManipulator(randomizeAddresses);
            default:
                throw new ArgumentException($"Unknown flow format: '{flowFormat}'.");
        }
    }
}


