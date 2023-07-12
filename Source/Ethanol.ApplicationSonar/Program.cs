// This is simple:
// Read input JSON, perform searching for IPs, kewords in domain names, etc, and produce the output...
//
// The Netify is provided as input XXX file and represented as in-memory database.
// The app fingeprint is also loaded from XXX file and represented as in-memory database NMemory (???)
// 

using ConsoleAppFramework;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
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
        [Command("Run", "Runs the tool, consuming context JSON file from stdin and producing results to stdout.")]
        public async Task RunAsync(
        [Option("i", "The URI or local path of the file with Internet address indicator file.")]
                string ipsUri,
        [Option("i", "The URI or local path of the file with known Internet Application list.")]
                string apsUri,
        [Option("a", "The URI or local path of the file with network application signatures.")]
                string signatureFileUri,
        [Option("f", "The format of the output. It can be JSON (default) or YAML.")]
                string outputFormat
        )
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Initializing...");
            logger.Info("Loading Internet Service database...");
            var serviceDb = await LoadServiceDatabaseAsync(ipsUri, apsUri);
            logger.Info($"Service Database Loaded: {serviceDb.AddressCount} Addresses, {serviceDb.ApplicationCount} Applications");

            logger.Info("Loading Application Signature database...");
            LoadSignatureDatabase(signatureFileUri);
            logger.Info("Application Signatire Databse loaded:");

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
            throw new NotImplementedException();
        }

        private Task<InternetApplicationDatabase> LoadServiceDatabaseAsync(string ipsUri, string apsUri)
        { 
            return InternetApplicationDatabase.LoadFromAsync(GetUri(ipsUri), GetUri(apsUri));
        }
    }
}