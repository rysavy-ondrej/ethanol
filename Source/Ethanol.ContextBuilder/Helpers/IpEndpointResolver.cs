using System;
using System.Linq;
using System.Net;

/// <summary>
/// Provides utility methods for resolving IPEndPoints from string representations.
/// </summary>
public static class IPEndPointResolver
{
    /// <summary>
    /// Resolves an <see cref="IPEndPoint"/> from the provided input string.
    /// </summary>
    /// <param name="input">A string representation of an IP endpoint in the format "host:port" or just "host".</param>
    /// <param name="defaultPort">The default port to use if the input does not specify a port.</param>
    /// <returns>The resolved <see cref="IPEndPoint"/> based on the provided input and default port.</returns>
    /// <exception cref="ArgumentException">Thrown when the input string contains more than one colon.</exception>
    public static IPEndPoint GetIPEndPoint(string input, int defaultPort = 0)
    {
        string[] parts = input.Split(':');
        if (parts.Length > 2)
        {
            throw new ArgumentException("Input string must contain at most one colon to separate the host and port.");
        }

        IPAddress? address;
        if (!IPAddress.TryParse(parts[0], out address))
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(parts[0]);
            address = hostEntry.AddressList.FirstOrDefault() ?? IPAddress.None;
        }
 
        int resolvedPort = parts.Length == 2 ? int.Parse(parts[1]) : defaultPort;
       
        return new IPEndPoint(address, resolvedPort);
    }
}
