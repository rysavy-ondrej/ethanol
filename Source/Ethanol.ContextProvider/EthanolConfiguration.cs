
/// <summary>
/// Represents the configuration settings required for the Ethanol application.
/// </summary>
public class EthanolConfiguration
{
    /// <summary>
    /// Gets or sets the computer name where the application is hosted.
    /// </summary>
    public string? ComputerName { get; set; }

    /// <summary>
    /// Gets or sets the hostname or IP address of the PostgreSQL server.
    /// </summary>
    public string? PostgresHost { get; set; }

    /// <summary>
    /// Gets or sets the port number on which the PostgreSQL server is listening.
    /// </summary>
    public string? PostgresPort { get; set; }

    /// <summary>
    /// Gets or sets the username used to authenticate with the PostgreSQL server.
    /// </summary>
    public string? PostgresUser { get; set; }

    /// <summary>
    /// Gets or sets the password used to authenticate with the PostgreSQL server.
    /// </summary>
    public string? PostgresPassword { get; set; }

    /// <summary>
    /// Gets or sets the name of the PostgreSQL database to connect to.
    /// </summary>
    public string? PostgresDatabase { get; set; }

    /// <summary>
    /// Gets or sets the name of the table storing host context data.
    /// </summary>
    public string? HostContextTable { get; set; }

    /// <summary>
    /// Gets or sets the name of the table storing tag data.
    /// </summary>
    public string? TagsTable { get; set; }

    public string GetConnectionString()
    { 
        var connectionString = $"Host={PostgresHost};Port={PostgresPort};Username={PostgresUser};Password={PostgresPassword};Database={PostgresDatabase}";
        return connectionString;
    }
}
