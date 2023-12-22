using System;
using System.Net;
using System.Net.Sockets;
using Ethanol.ContextBuilder.Context;
using System.Text.RegularExpressions;
using Ethanol.DataObjects;
using Ethanol.ContextBuilder.Readers.DataObjects;

/// <summary>
/// Provides methods for converting various data types used in mapping.
/// </summary>
public static class MapConvert
{
    /// <summary>
    /// Decodes a string by removing escape characters and trimming null characters.
    /// </summary>
    /// <param name="encodedString">The encoded string to decode.</param>
    /// <returns>The decoded string.</returns>
    public static string? DecodeString(string? encodedString)
    {
        if (encodedString == null) return null;
        return Regex.Unescape(encodedString).Trim('\0');
    }
    /// <summary>
    /// Decodes a string representation of a short array into an actual ushort array.
    /// </summary>
    /// <param name="stringArray">The string representation of the short array.</param>
    /// <returns>The decoded ushort array.</returns>
    public static ushort[]? DecodeShortArray(string? stringArray)
    {
        if (stringArray == null) return null;

        var hexString = stringArray.StartsWith("0x") ? stringArray[2..] : stringArray;

        ushort[] ushortArray = new ushort[hexString.Length / 4];

        for (int i = 0; i < hexString.Length; i += 4)
        {
            string hexSubstring = hexString.Substring(i, 4);
            ushortArray[i / 4] = System.Convert.ToUInt16(hexSubstring, 16);
        }
        return ushortArray;
    }
    /// <summary>
    /// Decodes a string representation of a byte array into an actual byte array.
    /// </summary>
    /// <param name="stringArray">The string representation of the byte array.</param>
    /// <returns>The decoded byte array.</returns>
    public static byte[]? DecodeByteArray(string? stringArray)
    {
        if (stringArray == null) return null;

        var hexString = stringArray.StartsWith("0x") ? stringArray[2..] : stringArray;

        byte[] byteArray = new byte[hexString.Length / 2];

        for (int i = 0; i < hexString.Length; i += 2)
        {
            string hexSubstring = hexString.Substring(i, 2);
            byteArray[i / 2] = System.Convert.ToByte(hexSubstring, 16);
        }
        return byteArray;
    }

    /// <summary>
    /// Converts a Unix timestamp to a DateTime object.
    /// </summary>
    /// <param name="unixTimestamp">The Unix timestamp to convert.</param>
    /// <returns>The DateTime object representing the Unix timestamp.</returns>
    public static DateTime UnixTimestamp(long unixTimestamp)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
        return dateTimeOffset.DateTime;
    }
    /// <summary>
    /// Converts a value representing microseconds into a TimeSpan.
    /// </summary>
    /// <param name="microseconds">The value in microseconds.</param>
    /// <returns>A TimeSpan representing the specified number of microseconds.</returns>
    public static TimeSpan Microseconds(long microseconds)
    {
        return TimeSpan.FromMicroseconds(microseconds);
    }

    internal static string? StripPrefix(string? value)
    {
        if (value == null) return null;
        return value.StartsWith("0x") ? value[2..] : value;
    }

    internal static string HexString(int value, int width)
    {
        return value.ToString($"X{width}");
    }

    /// <summary>
    /// Provides utility methods for converting network-related data.
    /// </summary>
    public static class Flow
    {
        /// <summary>
        /// Converts a string representation of a layer 4 protocol to a <see cref="ProtocolType"/> enumeration value.
        /// </summary>
        /// <param name="l4Proto">The string representation of the layer 4 protocol.</param>
        /// <returns>The corresponding <see cref="ProtocolType"/> value if the conversion is successful; otherwise, <see cref="System.Net.Sockets.ProtocolType.Unknown"/>.</returns>
        public static ProtocolType ProtocolType(string? l4Proto)
        {
            if (l4Proto == null) return System.Net.Sockets.ProtocolType.Unknown;
            return Enum.TryParse<ProtocolType>(l4Proto, true, out var proto) ? proto : System.Net.Sockets.ProtocolType.Unknown;
        }

        /// <summary>
        /// Converts the specified IP address version and values into an <see cref="IPAddress"/> object.
        /// </summary>
        /// <param name="ipVersion">The IP address version (4 or 6).</param>
        /// <param name="value4">The value for IPv4 address.</param>
        /// <param name="value6">The value for IPv6 address.</param>
        /// <returns>An <see cref="IPAddress"/> object representing the specified IP address version and values. 
        /// It returns IPAddress.None or IPAddress.IPv6None if address cannot be parsed or both inputs are null.
        /// </returns>
        public static IPAddress Address(int ipVersion, string? value4, string? value6)
        {
            if (value4 == null && value6 == null) return IPAddress.None;
            return ipVersion switch
            {
                4 => IPAddress.TryParse(value4, out var ipv4) ? ipv4 : IPAddress.None,
                6 => IPAddress.TryParse(value6, out var ipv6) ? ipv6 : IPAddress.IPv6None,
                _ => IPAddress.None
            };
        }

        /// <summary>
        /// Represents the application protocol used in the mapping.
        /// </summary>
        internal static ApplicationProtocol ApplicationName(int ianaApplicationId)
        {    
            switch (ianaApplicationId)        
            {
                case 50332091: return ApplicationProtocol.HTTPS;
                case 50331701: return ApplicationProtocol.DNS;
                case 50331728: return ApplicationProtocol.HTTP;
                case 218104261: return ApplicationProtocol.SSL;
                default: return ApplicationProtocol.Other;
            }
        }
    }
    /// <summary>
    /// Provides utility methods for HTTP-related operations.
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Converts an integer value to a string representation.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <returns>The string representation of the integer value.</returns>
        public static string MethodMaskString(int value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts the given value to a string representation.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The string representation of the value.</returns>
        public static string ResultCode(int value)
        {
            return ((HttpStatusCode)value).ToString();
        }

        /// <summary>
        /// Converts a string value to a URL string.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The converted URL string. If input value is null it returns an empty string.</returns>
        public static string UrlString(string? value)
        {
            if (value == null) return string.Empty;
            return value.ToString();
        }

        /// <summary>
        /// Converts an integer value to its string representation.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <returns>The string representation of the integer value.</returns>
        public static string OsString(int value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Converts an integer value to a string representation.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <returns>The string representation of the integer value.</returns>
        public static string UaString(int value)
        {
            return value.ToString();
        }
    }
    public static class Dns
    {
        /// <summary>
        /// Converts an integer value to a <see cref="DnsClass"/> enumeration value.
        /// </summary>
        /// <param name="recordClass">The integer value representing the DNS record class.</param>
        /// <returns>The corresponding <see cref="DnsClass"/> enumeration value.</returns>
        public static DnsClass RecordClass(int recordClass)
        {
            return (DnsClass)recordClass;
        }

        /// <summary>
        /// Converts an integer value to a <see cref="DnsRecordType"/> enumeration value.
        /// </summary>
        /// <param name="recordType">The integer value representing the DNS record type.</param>
        /// <returns>The corresponding <see cref="DnsRecordType"/> enumeration value.</returns>
        public static DnsRecordType RecordType(int recordType)
        {
            return (DnsRecordType)recordType;
        }

        /// <summary>
        /// The RCODE field is a 4-bit value located in the lower 4 bits of the flags field.
        /// </summary>
        /// <param name="flags">The flags value is obtained from the DNS message header. </param>
        /// <returns></returns>
        public static DnsResponseCode ResponseCode(int flags)
        {
            ushort rcode = (ushort)(flags & 0x0F);
            return (DnsResponseCode)rcode;
        }

        /// <summary>
        /// Converts the DNS flags to a string representation.
        /// </summary>
        /// <param name="flags">The DNS flags.</param>
        /// <returns>The string representation of the DNS flags.</returns>
        internal static string DnsFlags(int flags)
        {
            return flags.ToString();
        }

        /// <summary>
        /// The opcode field is a 4-bit value located in bits 11 to 14 of the flags field.
        /// </summary>
        /// <param name="flags">The flags value is obtained from the DNS message header. </param>
        /// <returns></returns>
        public static DnsOpCode DnsOpcode(int flags)
        {
            ushort opcode = (ushort)((flags >> 11) & 0x0F);
            return (DnsOpCode)opcode;
        }
        /// <summary>
        /// The QR bit is located in bit 15 of the flags field.
        /// </summary>
        /// <param name="flags">The flags value is obtained from the DNS message header. </param>
        /// <returns></returns>
        internal static DnsQueryResponseFlag QueryResponse(int flags)
        {
            bool qrBit = ((flags >> 15) & 0x01) == 1;
            return qrBit ? DnsQueryResponseFlag.Response : DnsQueryResponseFlag.Query;
        }
        /// <summary>
        /// Decodes the DNS record data based on the record type.
        /// </summary>
        /// <param name="rstring">The encoded DNS record data.</param>
        /// <param name="rtype">The type of DNS record.</param>
        /// <returns>The decoded DNS record data as a string or an empty string.</returns>
        public static string DecodeDnsRecordData(string? rstring, int rtype)
        {
            var rbytes = DecodeByteArray(rstring);
            if (rbytes == null) return string.Empty;
            return RecordType(rtype) switch
            {
                DnsRecordType.A => rbytes.Length >= 4 ? new IPAddress(rbytes[..4]).ToString() : string.Empty,
                DnsRecordType.AAAA => rbytes.Length >= 16 ? new IPAddress(rbytes[..16]).ToString() : string.Empty,
                _ => string.Empty,
            };
        }
    }
}



