using NRules;
using NRules.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ethanol.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This is a demo showing the principles of Context-based analysis.");

            Artifact input = null;
            DataSource source = OpenDataSource("Data");




            var ctx = InitializeContext(input, source);
            var rules = LoadRules(Assembly.GetExecutingAssembly());

            var output = EvaluateContext(ctx, input, rules);
            foreach (var artifact in output)
            {
                AnalyseOutput(artifact);
            }
        }

        /// <summary>
        /// Creates data source from files in the specified folder.
        /// </summary>
        /// <param name="path">The path to the folder with data files.</param>
        /// <returns></returns>
        private static DataSource OpenDataSource(string path)
        {
            foreach(var file in Directory.GetFiles(path))
            {

            }
            var ds = new DataSource()
            throw new NotImplementedException();
        }

        private static IEnumerable<Artifact> EvaluateContext(Context ctx, Artifact input, RuleRepository rules)
        {
            //Compile rules
            var factory = rules.Compile();

            //Create a working session
            var session = factory.CreateSession();
            
            //Insert facts into rules engine's memory
            session.InsertAll(ctx.Facts);

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
                collection.Artifacts.Where(builder.GetPredicate(target)); 
            }
            return ctx;
        }
    }
}
