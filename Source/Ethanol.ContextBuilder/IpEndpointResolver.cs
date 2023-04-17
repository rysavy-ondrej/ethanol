using System;
using System.Net;

public static class IPEndPointResolver
{
    public static IPEndPoint GetIPEndPoint(string input, int defaultPort = 0)
    {
        string[] parts = input.Split(':');
        if (parts.Length > 2)
        {
            throw new ArgumentException("Input string must contain at most one colon to separate the host and port.");
        }

        IPAddress address;
        if (!IPAddress.TryParse(parts[0], out address))
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(parts[0]);
            address = hostEntry.AddressList[0];
        }

        int resolvedPort = (parts.Length == 2 ? int.Parse(parts[1]) : defaultPort);

        return new IPEndPoint(address, resolvedPort);
    }
}
