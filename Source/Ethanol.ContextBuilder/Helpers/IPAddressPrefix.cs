using System;
using System.Net;

/// <summary>
/// Stores a single IP address prefix. It provides <see cref="Match(System.Net.IPAddress)"/> method to test if a given IP address is within the current prefix.
/// </summary>
public class IPAddressPrefix
{
    public IPAddress Address { get; }
    public int PrefixLength { get; }

    public static bool TryParse(string cidrNotation, out IPAddressPrefix addressPrefix)
    {
        addressPrefix = null;
        if (string.IsNullOrEmpty(cidrNotation))
        {
            return false;
        }

        var parts = cidrNotation.Split('/');
        if (parts.Length != 2)
        {
            return false;
        }

        if (IPAddress.TryParse(parts[0], out var address) && int.TryParse(parts[1], out var prefixLength))
        {
            if (prefixLength < 0 || (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && prefixLength > 32) ||
                (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && prefixLength > 128))
            {
                return false;
            }
            addressPrefix = new IPAddressPrefix(address, prefixLength);
            return true;
        }
        return false;
    }

    public IPAddressPrefix(IPAddress address, int prefix)
    {
        Address = address;
        PrefixLength = prefix;
        if (PrefixLength < 0 || (Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && PrefixLength > 32) ||
            (Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && PrefixLength > 128))
        {
            throw new ArgumentOutOfRangeException(nameof(address), "Prefix length is out of range.");
        }
    }
    public bool Match(IPAddress ipAddress)
    {
        if (ipAddress.AddressFamily != Address.AddressFamily)
        {
            return false;
        }

        byte[] addressBytes = Address.GetAddressBytes();
        byte[] candidateBytes = ipAddress.GetAddressBytes();
        byte[] maskBytes = GetMaskBytes(PrefixLength, addressBytes.Length);

        for (int i = 0; i < addressBytes.Length; i++)
        {
            if ((addressBytes[i] & maskBytes[i]) != (candidateBytes[i] & maskBytes[i]))
            {
                return false;
            }
        }
        return true;
    }

    private byte[] GetMaskBytes(int prefixLength, int length)
    {
        byte[] maskBytes = new byte[length];
        int bitsSet = 0;
        for (int i = 0; i < length; i++)
        {
            for (int bit = 7; bit >= 0 && bitsSet < prefixLength; bit--, bitsSet++)
            {
                maskBytes[i] |= (byte)(1 << bit);
            }
        }
        return maskBytes;
    }

    public override string ToString()
    {
        return $"{Address}/{PrefixLength}";
    }
}