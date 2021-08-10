using System;
using System.Net;

namespace Ethanol.Demo
{
    public static class IpfixArtifactExtension
    {
        public static IPAddress GetSourceAddress(this IpfixArtifact artifact) => IPAddress.TryParse(artifact.SrcIp, out var value) ? value : null;
        
        public static IPAddress GetDestinationAddress(this IpfixArtifact artifact) => IPAddress.TryParse(artifact.DstIp, out var value) ? value : null;

        public static DateTime GetStart(this IpfixArtifact artifact) => DateTime.TryParse(artifact.FirstSeen, out var value) ? value : DateTime.MinValue;

        public static TimeSpan GetDuration(this IpfixArtifact artifact) => TimeSpan.TryParse(artifact.Duration, out var value) ? value : TimeSpan.Zero;
    }
}