using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NRules;
using NRules.Fluent;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    class Program : ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder().ConfigureServices(ConfigureServices).RunConsoleAppFrameworkAsync<Program>(args);
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {            
            services.AddSingleton<ArtifactServiceCollection>();         
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
            var artifactServiceFactory = this.Context.ServiceProvider.GetService<ArtifactServiceCollection>();

            OpenDataSources(dataPath, artifactServiceFactory);
            var artifactServices = artifactServiceFactory.Build();

            var source = artifactServices.GetService(artifactServiceFactory.GetArtifactTypeByName(targetType)) as IArtifactProvider;                        
            var input = source?.GetQueryable<IpfixArtifact>().FirstOrDefault(f => f.Id == flowId);
            if (input == null)
            {
                Console.Error.WriteLine($"Flow id={flowId} not found in '{targetType}'");
                return;
            }
            var ctx = InitializeContext(input, artifactServices);
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

        /// <summary>
        /// Creates data sources from files in the specified folder.
        /// </summary>
        /// <param name="path">The path to the folder with data files.</param>
        /// <returns></returns>
        private static void OpenDataSources(string path, ArtifactServiceCollection artifactServiceFactory)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                var artifactType = artifactServiceFactory.GetArtifactTypeByName(Path.GetFileNameWithoutExtension(file));
                if (artifactType != null)
                {
                    artifactServiceFactory.AddArtifactProvider(artifactType, s => CsvArtifactProvider.CreateArtifactSource(artifactType, file));
                }
            }
        }

        private static IEnumerable<IpfixArtifact> EvaluateContext(Context ctx, IpfixArtifact input, RuleRepository rules)
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

        private static Context InitializeContext(IpfixArtifact target, ArtifactServiceProvider artifactServiceProvider)
        {
            var ctx = new Context();
            target.LoadToContext(ctx, artifactServiceProvider);
            return ctx;
        }
    }
}
