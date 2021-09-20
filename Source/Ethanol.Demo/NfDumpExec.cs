using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    public class NfDumpExec
    {
        private readonly ILogger<NfDumpExec> _logger;
        private readonly string _exePath = "/usr/local/bin/nfdump";
        public NfDumpExec(ILogger<NfDumpExec> logger = null)
        {
            _logger = logger;
        }

        public async Task ProcessInputAsync(string sourceFile, string filter, Func<Stream, Task> reader)
        {
            var stream = await ExecuteCommand(sourceFile, filter);
            stream.Position = 0;
            await reader(stream);
            stream.Close();
        }

        private async Task<Stream> ExecuteCommand(string sourceFile, string filter)
        {
            var stream = new MemoryStream();
            var sourceFileName = Path.GetFileName(sourceFile);
            var startInfo = new ProcessStartInfo()
            {
                FileName = _exePath,
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
            _logger?.LogTrace($"Executing command: {startInfo.FileName} {string.Join(" ", startInfo.ArgumentList)}");

            var tcs = new TaskCompletionSource();
            var outputCompleted = false;
            process.OutputDataReceived += (sender, data) =>
            {
                if (data.Data == null)
                {
                    //Console.Write($"...eof...");
                    tcs.SetResult();
                    return;
                }

                if (data.Data.StartsWith("Summary"))
                {
                    //Console.Write($"...stop...");
                    outputCompleted = true;
                }

                if (!outputCompleted && !String.IsNullOrWhiteSpace(data.Data))
                {
                    stream.Write(Encoding.UTF8.GetBytes(data.Data));
                    stream.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
                }
            };
            process.ErrorDataReceived += (sender, data) => { };
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                var processTask  = process.WaitForExitAsync();
                await Task.WhenAll(processTask, tcs.Task);
                sw.Stop();
                _logger?.LogTrace($"Command execution finished (duration: {sw.Elapsed}).");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Execution of command failed: {ex.Message}");
            }
            return stream;
        }
    }
}
