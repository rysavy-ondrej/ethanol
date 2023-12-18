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
        public void RunService(EthanolConfiguration configuration, ILogger logger)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddFastEndpoints();

            builder.Services.AddNpgsqlDataSource(configuration.GetConnectionString());
        
            builder.Services.AddSingleton<EthanolConfiguration>(configuration);

            builder.Services.AddSingleton<ILogger>(logger);

            var app = builder.Build();

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
                app.Run(configuration.ApplicationUrl);
                
            }
            catch (Exception e)
            {
                logger?.LogError($"FATAL: {e}");
            }  
        }
    }
}