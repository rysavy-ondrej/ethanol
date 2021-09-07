using ConsoleAppFramework;
using Ethanol.Artifacts;
using Ethanol.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NRules;
using NRules.Fluent;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    class Program : ConsoleAppBase
    {
        readonly ArtifactServiceCollection _artifactServiceCollection;

        public Program()
        {
            _artifactServiceCollection = new ArtifactServiceCollection(Assembly.GetExecutingAssembly());
        }

        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder().ConfigureServices(ConfigureServices).RunConsoleAppFrameworkAsync<Program>(args);
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {                     
        }

        [Command("create", "Creates a context for the specified flow object.")]
        public void CreateContext(
            [Option("p", "path to data folder with source csv files.")]
            string dataPath, 
            [Option("t", "type of the target flow artifact. It can be flow, dns, tls, http, samba or tcp.")]
            string targetType, 
            [Option("i", "flow id.")]
            string flowId)
        {
            
            Console.WriteLine($"Preparing context for {targetType} flow id={flowId}...");
            _artifactServiceCollection.AddArtifactFromCsvFiles(dataPath);
            var artifactServices = _artifactServiceCollection.Build();
            Console.WriteLine(String.Join(',', artifactServices.Services));

            var artifactType = _artifactServiceCollection.GetArtifactTypeByName(targetType);
            var source = artifactServices.GetService(artifactType);                        
            var input = source?.GetObservable<IpfixArtifact>().FirstOrDefaultAsync(f => f.Id == flowId).Wait();
            if (input == null)
            {
                Console.Error.WriteLine($"Flow id={flowId} not found in '{targetType}'");
                return;
            }

            var ctx = new ContextSet();
            input.LoadToContext(ctx, artifactServices, GetFactLoaders(artifactType));

            var writer = new IndentedTextWriter(Console.Out, "  ");
            writer.WriteLine("Artifact:");
            writer.Indent += 2;
            input.DumpYaml(writer);
            writer.Indent -= 2;
            Console.WriteLine("Context:");
            writer.Indent += 2;
            ctx.DumpYaml(writer);
            /*
            var rules = LoadRules(Assembly.GetExecutingAssembly());

            var output = EvaluateContext(ctx, input, rules);
            foreach (var artifact in output)
            {
                AnalyseOutput(artifact);
            }
            */
        }

        private FactLoader[] GetFactLoaders(Type targetType)
        {
            var loaders = new FactLoader[] {
                LoaderFunctions.ServiceDomain<ArtifactLong>(TimeSpan.FromMinutes(5))
                               .GetLoader()
            };
            return loaders;
        }

        private static IEnumerable<IpfixArtifact> EvaluateContext(ContextSet ctx, IpfixArtifact input, RuleRepository rules)
        {
            //Compile rules
            var factory = rules.Compile();

            //Create a working session
            var session = factory.CreateSession();
            
            //Insert facts into rules engine's memory
            session.InsertAll(ctx.Facts.Select(x=>(object)x.Value));

            //Start match/resolve/act cycle
            session.Fire();

            //Here we have a result...
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executed to perform analysis of the rich artifact.
        /// </summary>
        /// <param name="ctx"></param>
        private static void AnalyseOutput(IpfixArtifact ctx)
        {
            throw new NotImplementedException();
        }

        private static RuleRepository LoadRules(Assembly assembly)
        {
            var repository = new RuleRepository();
            repository.Load(x => x.From(assembly));
            return repository;
        }
    }
}
