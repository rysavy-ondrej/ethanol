using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;

namespace Ethanol.Demo
{
    /// <summary>
    /// Observers the source folder with nfdump files and triggers processing of newly added files.
    /// </summary>
    public class NetflowDumpMonitor : IObservable<CsvSourceFile>, IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly string _inPath;
        private readonly string _outPath;
        private readonly SourceRecipe[] _recipes;
        private readonly IObservable<CsvSourceFile> _observable;
        private event Action<CsvSourceFile> SourceFileCreated;
        /// <summary>
        /// Creates a new instance watching the specified folder for newly created files.
        /// </summary>
        /// <param name="inPath"></param>
        public NetflowDumpMonitor(string inPath, string outPath, params SourceRecipe[] recipes)
        {
            _inPath = inPath ?? throw new ArgumentNullException(nameof(inPath));
            this._outPath = outPath;
            _recipes = recipes;
            _watcher = new FileSystemWatcher
            {
                Path = _inPath,
                Filter = "*.*",
                EnableRaisingEvents = true
            };
            _watcher.Created += new FileSystemEventHandler(OnCreated);

            _observable = Observable.FromEvent<CsvSourceFile>(
                fsHandler => SourceFileCreated += fsHandler,
                fsHandler => SourceFileCreated -= fsHandler);
        }
        /// <summary>
        /// Called when a new file was created in the given folder.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnCreated(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"New flow dump observed: {e.FullPath}.");

            foreach (var filter in _recipes)
            {
                var filename = Path.GetFileName(e.FullPath);
                var outputFile = Path.Combine(_outPath, $"{filename}.{filter.ArtifactName}");
                var success = NfDump.Execute(e.FullPath, outputFile, filter.FilterExpression);
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

        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}
