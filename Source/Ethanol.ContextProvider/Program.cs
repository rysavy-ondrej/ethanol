using Cysharp.Text;
using Ethanol.ContextBuilder;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using ZLogger;

// TODO:
// input arguments are:
// local port to which the program listents to requests
// connection string to PostgreSQL database
// name of the HOST-CONTEXT table
// name of the TAG table



var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints();

var npgConnectionString = bld.Configuration.GetConnectionString("DefaultConnection");
if (npgConnectionString == null) throw new Exception("Missing connectin string.");

bld.Services.AddNpgsqlDataSource(npgConnectionString);

bld.Services.AddLogging(logging =>
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


var app = bld.Build();

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
LogManager.SetLoggerFactory(loggerFactory, "Global");


app.UseFastEndpoints(c =>
{
    // HACK: here enable anonymous access for every endpoint...
    c.Endpoints.Configurator = ep =>
    {
        ep.AllowAnonymous();            
    };
});

app.Run();