using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.Demo
{
    /// <summary>
    /// Observers the source folder with nfdump files and triggers processing of newly added files.
    /// </summary>
    public class NetflowDumpProcessor : IObservable<CsvSourceFile>
    {
        private readonly string _inPath;
        private readonly string _outPath;
        private readonly SourceRecipe[] _recipes;
        private readonly IObservable<CsvSourceFile> _observable;
        private event Action<CsvSourceFile> SourceFileCreated;
        /// <summary>
        /// Creates a new instance watching the specified folder for newly created files.
        /// </summary>
        /// <param name="inPath"></param>
        public NetflowDumpProcessor(string inPath, string outPath, params SourceRecipe[] recipes)
        {
            _inPath = inPath ?? throw new ArgumentNullException(nameof(inPath));
            this._outPath = outPath;
            _recipes = recipes;


            _observable = Observable.FromEvent<CsvSourceFile>(
                fsHandler => SourceFileCreated += fsHandler,
                fsHandler => SourceFileCreated -= fsHandler);

            var dumpFiles = Directory.GetFiles(inPath);
            foreach (var dumpFile in dumpFiles.OrderBy(x => x))
            {
                OnCreated(this, new FileSystemEventArgs(WatcherChangeTypes.All, inPath, dumpFile));
            }
        }
        /// <summary>
        /// Called when a new file was created in the given folder.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnCreated(object source, FileSystemEventArgs e)
        {
            var sourceFilename = e.Name;
            Console.WriteLine($"Processing flow dump: {e.Name}.");

            foreach (var filter in _recipes)
            {
                var filename = Path.GetFileName(sourceFilename);
                var outputFile = Path.Combine(_outPath, $"{filename}.{filter.ArtifactName}");
                var success = NfDump.Execute(sourceFilename, outputFile, filter.FilterExpression);
                if (success)
                {
                    Console.WriteLine("OK: CSV file generated.");
                    SourceFileCreated?.Invoke(new CsvSourceFile(filter, outputFile));
                }
                else
                {
                    Console.WriteLine("ERROR: nfdump failed.");
                }
            }
        }

        public IDisposable Subscribe(IObserver<CsvSourceFile> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
