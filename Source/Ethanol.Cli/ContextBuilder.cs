using Microsoft.Extensions.Logging;
using System.Text;
using Ethanol.Catalogs;
using System.Net;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Writers;
using Ethanol.ContextBuilder.Builders;
using Ethanol.ContextBuilder.Enrichers;
using System.Data;
using Ethanol.ContextBuilder.Pipeline;
using Ethanol.ContextBuilder.Polishers;
using Ethanol.ContextBuilder.Observable;
using Npgsql;
using System.Linq.Expressions;

namespace Ethanol.Cli
{


    internal class ContextBuilder : ConsoleAppBase
    {
        private readonly ILogger _logger;
        private readonly EthanolEnvironment _environment;

        public ContextBuilder(ILogger<ContextBuilder> logger, EthanolEnvironment environment)
        {
            this._logger = logger;
            this._environment = environment;
        }

        [Command("run-builder", "Starts the application.")]
        public async Task RunBuilderCommand(

        [Option("c", "The configuration file used to configure the processing.")]
                string configurationFile
        )
        {
            // this is our internal cnacellation token:
            var cts = new CancellationTokenSource();
            var progressReport = new BuilderProgressReport();

            // Load configuration:
            var configurationFilePath = Path.GetFullPath(configurationFile);
            _logger?.LogInformation($"Running context builder with configuration file: '{configurationFilePath}'");
            var contextBuilderConfiguration = ContextBuilderConfiguration.LoadFromFile(configurationFilePath);

            // create modules: 
            var readers = CreateInputReaders(contextBuilderConfiguration.Input, _environment);
            var writers = CreateOutputWriters(contextBuilderConfiguration.Output, _environment);
            var builder = CreateBuilder(contextBuilderConfiguration.Builder, _environment);
            var enricher = CreateEnricher(contextBuilderConfiguration.Enrichers, _environment);
            var polisher = _environment.ContextTransform.GetContextPolisher();
            // set-up pipeline:
            var pipeline = _environment.ContextBuilder.CreateIpHostContextBuilderPipeline(readers, writers, builder, enricher, polisher, x => progressReport.ConsumedFlows += x, y => progressReport.ProducedContexts += y);






            var monitorTask = MonitorProcessingAsync(progressReport, cts.Token);

            // execute:
            await pipeline.Start(this.Context.CancellationToken).ContinueWith(t => cts.Cancel());
            
            await monitorTask;
        }



        private async Task MonitorProcessingAsync(BuilderProgressReport progressReport, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    _logger?.LogInformation($"Status: consumed flows={progressReport.ConsumedFlows}, produced contexts={progressReport.ProducedContexts}.");
                    await Task.Delay(10000, ct);
                }
            }
            catch (TaskCanceledException) { }
        }

        private IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> CreateEnricher(ContextBuilderConfiguration.Enrichers enricher, EthanolEnvironment environment)
        {
            if (enricher?.Netify != null && enricher.Netify.Postgres != null)
            {
                return environment.ContextTransform.GetNetifyPostgresEnricher(enricher.Netify.Postgres.GetConnectionString(), enricher.Netify.Postgres.TableName);
            } 
            return environment.ContextTransform.GetVoidEnricher();
        } 

        private IpHostContextBuilder? CreateBuilder(ContextBuilderConfiguration.ContextBuilder builder, EthanolEnvironment environment)
        {
            var prefixes = builder.Networks.Select(s => IPAddressPrefix.TryParse(s, out var prefix) ? prefix : null).Where(x => x != null).ToArray();
            var filter = (prefixes.Length != 0) ? new HostBasedFilter(prefixes) : new HostBasedFilter();
            return environment.ContextBuilder.GetIpHostContextBuilder(TimeSpan.TryParse(builder.WindowSize, out var ts)? ts : TimeSpan.FromMinutes(5), 
                TimeSpan.TryParse(builder.WindowHop, out var th) ? th : TimeSpan.FromMinutes(5), filter);

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

        class BuilderProgressReport
        {
            public int ConsumedFlows { get; set; }

            public int ProducedContexts { get; set; }
        }
    }
}