using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using NRules;
using NRules.Fluent;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    class Program : ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
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

            DataSource source = OpenDataSource(dataPath);
            source.Validate();
            var targetArtifactType = ArtifactFactory.GetArtifact(targetType);
            Artifact input = source.GetArtifactSource(targetArtifactType).Artifacts.FirstOrDefault(f => f.Id == flowId);
            if (input == null)
            {
                Console.Error.WriteLine($"Flow id={flowId} not found in '{targetType}'");
                return;
            }
            var ctx = InitializeContext(input, source);

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
        /// Creates data source from files in the specified folder.
        /// </summary>
        /// <param name="path">The path to the folder with data files.</param>
        /// <returns></returns>
        private static DataSource OpenDataSource(string path)
        {
            ArtifactFactory.LoadArtifactsFromAssembly(Assembly.GetExecutingAssembly());
            IEnumerable<ArtifactSource> GetSources()
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    var artifactType = ArtifactFactory.GetArtifact(Path.GetFileNameWithoutExtension(file), StringComparison.InvariantCultureIgnoreCase);
                    yield return CsvArtifactSource.CreateArtifactSource(artifactType, file);
                }
            }
            var ds = new DataSource(GetSources());
            return ds;
        }

        private static IEnumerable<Artifact> EvaluateContext(Context ctx, Artifact input, RuleRepository rules)
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
        private static void AnalyseOutput(Artifact ctx)
        {
            throw new NotImplementedException();
        }

        private static RuleRepository LoadRules(Assembly assembly)
        {
            var repository = new RuleRepository();
            repository.Load(x => x.From(assembly));
            return repository;
        }

        private static Context InitializeContext(Artifact target, DataSource source)
        {
            // load artifacts from source...
            var ctx = new Context();
            
            foreach (var builder in target.Builders)
            {
                var collection = source.GetArtifactSource(builder.OutputType);
                var facts = collection.Artifacts.Where(builder.GetPredicate(target));
                foreach (var fact in facts)
                {
                    ctx.Add(builder.Name, fact);
                }
            }
            return ctx;
        }
    }
}
