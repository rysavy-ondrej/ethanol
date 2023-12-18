
namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents information about a web request, including details about the remote host, port, and URL.
    /// </summary>
    /// <param name="RemoteHostAddress">The remote host IP address.</param>
    /// <param name="RemoteHostName">The remote host name.</param>
    /// <param name="RemotePort">The remote port number.</param>
    /// <param name="ApplicationProcessName">The name of the application or process that made the request.</param>
    /// <param name="Method">The HTTP method used in the request (e.g. GET, POST, etc.).</param>
    /// <param name="Url">The URL of the requested resource.</param>
    public record WebRequestInfo(string RemoteHostAddress, string? RemoteHostName, ushort RemotePort, string? ApplicationProcessName, InternetServiceTag[]? InternetServices, string Method, string Url);


}
