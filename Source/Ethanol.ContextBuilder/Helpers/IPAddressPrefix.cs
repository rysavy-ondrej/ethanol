﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Ethanol.ContextBuilder.Helpers
{

    /// <summary>
    /// Represents an IP address combined with a network prefix length, often used to specify subnets.
    /// </summary>
    public class IPAddressPrefix
    {
        /// <summary>
        /// Gets the IP address part of the prefix.
        /// </summary>
        public IPAddress Address { get; }
        /// <summary>
        /// Gets the length of the prefix or subnet mask.
        /// </summary>
        public int PrefixLength { get; }

        /// <summary>
        /// Attempts to parse a CIDR notation string into an <see cref="IPAddressPrefix"/> object.
        /// </summary>
        /// <param name="cidrNotation">The CIDR notation string to parse, e.g., "192.168.1.0/24".</param>
        /// <param name="addressPrefix">When this method returns, contains the <see cref="IPAddressPrefix"/> equivalent of the CIDR notation contained in <paramref name="cidrNotation"/>, if the conversion succeeded, or null if the conversion failed.</param>
        /// <returns><c>true</c> if <paramref name="cidrNotation"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string cidrNotation, [NotNullWhen(true)] out IPAddressPrefix? addressPrefix)
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
                if (prefixLength < 0 || address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && prefixLength > 32 ||
                    address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && prefixLength > 128)
                {
                    return false;
                }
                addressPrefix = new IPAddressPrefix(address, prefixLength);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressPrefix"/> class using a specified IP address and prefix length.
        /// </summary>
        /// <param name="address">The IP address part of the prefix.</param>
        /// <param name="prefix">The length of the network prefix or subnet mask.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided prefix length is out of valid range for the IP address family.</exception>
        public IPAddressPrefix(IPAddress address, int prefix)
        {
            Address = address;
            PrefixLength = prefix;
            if (PrefixLength < 0 || Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && PrefixLength > 32 ||
                Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && PrefixLength > 128)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Prefix length is out of range.");
            }
        }

        /// <summary>
        /// Determines whether the provided IP address matches the current address prefix.
        /// </summary>
        /// <param name="ipAddress">The IP address to check against the prefix.</param>
        /// <returns><c>true</c> if the provided IP address matches the current prefix; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// The method checks if the provided IP address belongs to the same network as defined by the prefix.
        /// It does so by applying the subnet mask to both IP addresses and comparing the results.
        /// </remarks>
        public bool Match(IPAddress? ipAddress)
        {

            if (ipAddress == null || ipAddress.AddressFamily != Address.AddressFamily)
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

        /// <summary>
        /// Generates the subnet mask as a byte array from the provided prefix length and total length.
        /// </summary>
        /// <param name="prefixLength">The prefix length of the subnet mask.</param>
        /// <param name="length">The total length of the mask in bytes.</param>
        /// <returns>The byte array representation of the subnet mask.</returns>
        /// <remarks>
        /// The subnet mask is generated by setting the leftmost bits in the byte array according to the prefix length.
        /// </remarks>
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

        /// <summary>
        /// Returns a string representation of the IP address prefix in CIDR notation.
        /// </summary>
        /// <returns>A string in the format "IPAddress/PrefixLength".</returns>
        /// <remarks>
        /// This method is especially useful for displaying the IP address prefix in a human-readable format.
        /// </remarks>
        public override string ToString()
        {
            return $"{Address}/{PrefixLength}";
        }
    }
}