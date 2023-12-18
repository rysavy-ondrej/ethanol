// This is simple:
// Read input JSON, perform searching for IPs, kewords in domain names, etc, and produce the output...
//
// The Netify is provided as input XXX file and represented as in-memory database.
// The app fingeprint is also loaded from XXX file and represented as in-memory database NMemory (???)
// 

using Ethanol.DataObjects;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Text.Json;

namespace Ethanol.ApplicationSonar
{
    /// <summary>
    /// The program class containing the main entry point.
    /// </summary>
    public class Program : ConsoleAppBase
    {

        /// <summary>
        /// Entry point to the console application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            try
            {
                AddLogging();
                ConsoleApp.Run<ProgramCommands>(args);
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal(ex, $"ERROR:{ex.Message}");
            }
        }

        static void AddLogging()
        {
            var config = new LoggingConfiguration();

            // CONSOLE LOGGING:
            var consoleTarget = new ColoredConsoleTarget("console")
            {
                UseDefaultRowHighlightingRules = true,
                DetectConsoleAvailable = true,
                DetectOutputRedirected = true,
                StdErr = true,
                Layout = "${longdate}|${level}|${message}"
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);


            var fileTarget = new FileTarget("file")
            {
                FileName = "Ethanol.ContextBuilder.trace"
            };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);


            NLog.LogManager.Configuration = config;


            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Logging has been setup.");
        }
    }

    internal class ProgramCommands : ConsoleAppBase
    {
        Logger _logger = LogManager.GetCurrentClassLogger();

        [Command("Run", "Runs the tool, consuming context JSON file from stdin and producing results to stdout.")]
        public async Task RunAsync(
        [Option("i", "The URI or local path of the file with Internet address indicator file.")]
                string ipsUri,
        [Option("a", "The URI or local path of the file with known Internet Application list.")]
                string apsUri,
        [Option("p", "The URI or local path of the file with network process signatures.")]
                string processFileUri,
        [Option("f", "The format of the output. It can be JSON (default) or YAML.")]
                string outputFormat
        )
        {
            _logger?.Info("Initializing...");
            _logger?.Info("Loading Internet Service database...");
            var serviceDb = await LoadServiceDatabaseAsync(ipsUri, apsUri);
            _logger?.Info($"Service Database Loaded: {serviceDb.AddressCount} Addresses, {serviceDb.ApplicationCount} Applications");

            _logger?.Info("Loading Application Signature database...");
            LoadSignatureDatabase(processFileUri);
            _logger?.Info("Process Signature Databse loaded:");

            _logger?.Info("Processing input...");
            await ProcessInputAsync(serviceDb);

        }

        private async Task ProcessInputAsync(InternetApplicationDatabase serviceDb)
        {
            string? line;
            while ((line = await Console.In.ReadLineAsync()) != null)
            {

                try
                {
                    var hostRecord = JsonSerializer.Deserialize<HostContext>(line);
                    if (hostRecord != null)
                    {
                        _logger?.Info($"Processing context for host {hostRecord.Key}, [{hostRecord.Start} | {hostRecord.End}].");
                        // print output now just fo debug:
                        Console.WriteLine($"host: {hostRecord.Key}");
                        if (hostRecord.Connections != null)
                        {
                            foreach (var con in hostRecord.Connections)
                            {
                                var tls = GetTlsForConnection(hostRecord, con);
                                var remoteAddress = con.RemoteHostAddress;
                                var applications = serviceDb.GetApplications(remoteAddress, out var shared);

                                Console.WriteLine($"  connects-to: {{ address: {remoteAddress}, hostname: {con.RemoteHostName}, services: {con.InternetServices}, ciphers: {tls?.CipherSuites}, sni: {tls?.ServerNameIndication}, shared: {shared} }}");
                                Console.WriteLine($"     services:");
                                foreach (var app in applications)
                                {
                                    Console.WriteLine($"       - {app}");
                                }

                            }
                        }
                        else
                        {
                            Console.WriteLine("  no connections.");
                        }
                    }

                }
                catch(Exception e)
                {
                    _logger?.Error(e);
                }
            }
        }

        private static TlsHandshakeInfo? GetTlsForConnection(HostContext hostRecord, IpConnectionInfo con)
        {
            return hostRecord.TlsHandshakes?.FirstOrDefault(t => String.Equals(t.RemoteHostAddress, con.RemoteHostAddress) && t.RemotePort == con.RemotePort);
        }

        Uri GetUri(string input)
        {
            try
            {
                string absolutePath = Path.GetFullPath(input);
                var uri = File.Exists(absolutePath) ? new Uri(absolutePath) : new Uri(input);
                return uri;
            }
            catch (UriFormatException)
            {
                throw new FileNotFoundException("File not found or the access schema is invalid.", input);
            }
        }
        private void LoadSignatureDatabase(string signatureFileUri)
        {
           
        }

        private Task<InternetApplicationDatabase> LoadServiceDatabaseAsync(string ipsUri, string apsUri)
        { 
            return InternetApplicationDatabase.LoadFromAsync(GetUri(ipsUri), GetUri(apsUri));
        }
    }
}