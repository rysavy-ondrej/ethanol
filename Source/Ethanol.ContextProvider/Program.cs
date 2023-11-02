using ConsoleAppFramework;
using Cysharp.Text;
using Ethanol.ContextBuilder;
using FastEndpoints;
using ZLogger;

namespace Ethanol.ContextProvider
{

    /// <summary>
    /// The program class containing the main entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point to the console application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {

            var configurationFile = (args.Length == 1) ? args[0] : "appsettings.json";
            

            // >>>>>>>>>>>>>
            // CONFIGURATION
            // >>>>>>>>>>>>>
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddFastEndpoints();

            builder.Configuration.AddJsonFile(configurationFile, optional: false, reloadOnChange: true);
            builder.Services.Configure<EthanolConfiguration>(builder.Configuration.GetSection("EthanolConfiguration"));

            builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("Default"));


            builder.Services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddZLoggerConsole(options =>
                    {
                        options.PrefixFormatter = (writer, info) => ZString.Utf8Format(writer, "[{0}][{1}] ", info.LogLevel, info.Timestamp.DateTime.ToLocalTime());
                    }, outputToErrorStream: true);
#if DEBUG
                    logging.SetMinimumLevel(LogLevel.Trace);
#else
        logging.SetMinimumLevel(LogLevel.Information);
#endif

                });


            var app = builder.Build();

            // set up logger:
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            LogManager.SetLoggerFactory(loggerFactory, "Global");

            // adjust endpoints:
            app.UseFastEndpoints(c =>
            {
                // HACK: here enable anonymous access for every endpoint...
                c.Endpoints.Configurator = ep =>
                {
                    ep.AllowAnonymous();
                };
            });

            try
            {
                app.Run();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"FATAL: {e}");
            }
        }
    }
}