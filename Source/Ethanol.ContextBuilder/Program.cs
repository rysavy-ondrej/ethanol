﻿using Cysharp.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using ZLogger;

namespace Ethanol.ContextBuilder
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
            var builder = ConsoleApp.CreateBuilder(args);
            builder.ConfigureServices((ctx, services) =>
            {
                services.AddLogging(logging =>
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
            });

            var app = builder.Build();
            // get configured loggerfactory.
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            LogManager.SetLoggerFactory(loggerFactory, "Global");

            app.AddCommands<ProgramCommands>();
            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.LogCritical(ex, $"ERROR:{ex.Message}");
            }           
        }
    }
}
