using System;
using System.Net;

namespace Ethanol.Demo
{
    public static class ArtifactOperations
    {
        static bool IPAddressEquals(IPAddress addr1, IPAddress addr2)
        {
            return addr1.ToString() == addr2.ToString();
        }
        public static bool EndPoint(this IpfixArtifact x, IpfixArtifact y)
        {
            return (x.SrcIp == y.SrcIp) && (x.DstIp == y.DstIp);
        }

        public static bool SameSource(this IpfixArtifact x, IpfixArtifact y)
        {
            return (x.SrcIp == y.SrcIp);
        }
        public static bool EndPointConv(this IpfixArtifact x, IpfixArtifact y)
        {
            return (x.SrcIp == y.SrcIp) && (x.DstIp == y.DstIp)
                ||
                (x.SrcIp == y.DstIp) && (x.DstIp == y.SrcIp);
        }
        public static bool EndPoint(this IpfixArtifact source, string src2, string dst2)
        {
            return source.SrcIp == src2 && source.DstIp == dst2;
        }
        public static bool EndPointConv(this IpfixArtifact source, string src2, string dst2)
        {
            return (source.SrcIp == src2 && source.DstIp == dst2)
                ||
                    (source.SrcIp == dst2 && source.DstIp == src2);
        }
        /// <summary>
        /// Test if <paramref name="target"/> artifact occurs in window starting at <paramref name="source"/> artifact and the given <paramref name="interval"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="interval"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool After(this IpfixArtifact source, TimeSpan interval, IpfixArtifact target)
        {
            return target.GetStart() > source.GetStart() && target.GetStart() < source.GetStart() + interval;
        }
        public static bool Before(this IpfixArtifact source, TimeSpan interval, IpfixArtifact target)
        {
            return target.GetStart() > source.GetStart() - interval && target.GetStart() < source.GetStart();
        }
        public static bool Window(this IpfixArtifact source, TimeSpan before, TimeSpan after, IpfixArtifact target)
        {
            return target.GetStart() > source.GetStart() - before && target.GetStart() < source.GetStart() + after;
        }
    }
}