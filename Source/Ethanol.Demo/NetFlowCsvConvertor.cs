using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    /// <summary>
    /// Observers the source folder with nfdump files and triggers processing of newly added files.
    /// </summary>
    public class NetFlowCsvConvertor
    {
        private readonly ILogger _logger;
        private readonly ArtifactDataSource[] _sources;
        /// <summary>
        /// Creates a new instance watching the specified folder for newly created files.
        /// </summary>
        public NetFlowCsvConvertor(ILogger<NetFlowCsvConvertor> logger, params ArtifactDataSource[] sources)
        {
            _sources = sources;
            _logger = logger;
        }
        /// <summary>
        /// Generates CSV files from the given nfdump file using the ciurrent collection of <see cref="ArtifactDataSource"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<CsvSourceFile> Generate(FileInfo value)
        {
            _logger.LogTrace($"New flow dump observed: {value.FullName}.");
            foreach (var filter in _sources)
            {
                var ms = new MemoryStream(4096);
                using var outputWriter = new StreamWriter(ms, leaveOpen: true);
                var success = Execute(value.FullName, outputWriter, filter.FilterExpression);
                if (success)
                {
                    _logger.LogTrace($"Csv file generated and available in memory stream.");
                    outputWriter.Close();
                    ms.Seek(0, SeekOrigin.Begin);
                    yield return new CsvSourceFile(filter, value.FullName, ms);
                }
            }
        }

        /// <summary>
        /// Executes the nfdump for the given input file to produce a target csv file. 
        /// </summary>
        /// <param name="sourceFile">The source nfdump file.</param>
        /// <param name="targetFile">The target CSV file.</param>
        /// <param name="filter">An nfdump filter to be applied.</param>
        /// <returns>true on success; false otherwise.</returns>
        public bool Execute(string sourceFile, StreamWriter targetWriter, string filter)
        {
            var sourceFileName = Path.GetFileName(sourceFile);
            var startInfo = new ProcessStartInfo()
            {
                FileName = "/usr/local/bin/nfdump",
                ArgumentList = { "-R", $"{sourceFile}:{sourceFileName}", "-o", "csv", filter },
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process()
            {
                StartInfo = startInfo,
            };
            _logger.LogTrace($"Executing command: {startInfo.FileName} {string.Join(" ", startInfo.ArgumentList)}");

            process.OutputDataReceived += (sender, data) =>
            {
                targetWriter.WriteLine(data.Data);
            };
            process.ErrorDataReceived += (sender, data) =>
            {
                /* ignore */;
            };

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                sw.Stop();
                _logger.LogTrace($"Command execution finished (duration: {sw.Elapsed}).");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Execution of command failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Copletes  all observables  associated with s
        /// </summary>
        public void Close()
        {
            foreach (var source in _sources)
            {
                source.Close();
            }     
        }
    }
}
