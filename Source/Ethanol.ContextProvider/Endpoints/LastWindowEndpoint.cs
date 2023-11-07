using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Ethanol.ContextProvider.Endpoints
{

    /// <summary>
    /// Provides an endpoint to retrieve information about the last available time window.
    /// </summary>
    /// <remarks>
    /// The endpoint responds to GET requests on "/api/v1/host-context/windows/last" and returns the details of the last available time window.
    /// </remarks>
    [HttpGet("/api/v1/host-context/windows/last")]
    public class LastWindowEndpoint : EndpointWithoutRequest<WindowObject>
    {
        static protected readonly ILogger __logger = LogManager.GetCurrentClassLogger();

        private readonly NpgsqlDataSource _dataSource;
        private readonly EthanolConfiguration _configuration;

        public LastWindowEndpoint(NpgsqlDataSource datasource, EthanolConfiguration configuration)
        {
            _dataSource = datasource;
            _configuration = configuration;
        }


        /// <summary>
        /// Handles incoming requests and responds with the details of the last available time window.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Asynchronous task signalizing the status of the operation.</returns>
        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                using var conn = _dataSource.OpenConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT validity FROM {_configuration.HostContextTable} ORDER BY id DESC LIMIT 1";

                var reader = await cmd.ExecuteReaderAsync(ct);
                if (reader.Read())
                {
                    var validity = reader.GetFieldValue<NpgsqlRange<DateTime>>("validity");
                    await SendAsync(new WindowObject { Start = validity.LowerBound, End = validity.UpperBound });
                }
                else
                {
                    await SendAsync(default);
                }
                reader.Close();
            }
            catch(Exception ex)
            {
                __logger?.LogError(ex, $"Endpoint '{nameof(LastWindowEndpoint)}' cannot create a response.");
                await SendErrorsAsync(500, ct);
            }
        }
    }
}