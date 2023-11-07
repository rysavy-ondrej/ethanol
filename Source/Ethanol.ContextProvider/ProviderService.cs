using FastEndpoints;

namespace Ethanol.ContextProvider
{

    /// <summary>
    /// The program class containing the main entry point for the Context Provider Service.
    /// </summary>
    public class ProviderService
    {
        /// <summary>
        /// Entry point to the console application.
        /// </summary>
        public Task RunService(EthanolConfiguration configuration, ILogger? logger)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddFastEndpoints();

            builder.Services.AddNpgsqlDataSource(configuration.GetConnectionString());
        
            builder.Services.AddSingleton<EthanolConfiguration>(configuration);
            var app = builder.Build();

            // set up logger:
            LogManager.SetGlobalLogger(logger);

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
                // TODO: How to use RunAsync instead of Run?  
                app.Run(configuration.ApplicationUrl);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                logger?.LogError($"FATAL: {e}");
                return Task.FromException(e);
            }  
        }
    }
}