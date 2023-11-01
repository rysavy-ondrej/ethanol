using FastEndpoints;
using Microsoft.AspNetCore.Builder;

// TODO:
// input arguments are:
// local port to which the program listents to requests
// connection string to PostgreSQL database
// name of the HOST-CONTEXT table
// name of the TAG table

var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints();

var app = bld.Build();
app.UseFastEndpoints(c =>
{
    // HACK: here neable anonymous access for every endpoint...
    c.Endpoints.Configurator = ep =>
    {
        ep.AllowAnonymous();            
    };
});

app.Run();