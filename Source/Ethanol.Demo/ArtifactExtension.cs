using System;
using System.Net;

namespace Ethanol.Demo
{
    public static class ArtifactExtension
    {
        static bool IPAddressEquals(IPAddress addr1, IPAddress addr2)
        {
            return addr1.ToString() == addr2.ToString();
        }
        public static bool EndPoint(this Artifact x, Artifact y)
        {
            return IPAddressEquals(x.Source, y.Source) && IPAddressEquals(x.Destination, y.Destination);
        }
        public static bool EndPointConv(this Artifact x, Artifact y)
        {
            return IPAddressEquals(x.Source, y.Source) && IPAddressEquals(x.Destination, y.Destination)
                ||
                IPAddressEquals(x.Source, y.Destination) && IPAddressEquals(x.Destination, y.Source);
        }
        public static bool EndPoint(this Artifact source, string src2, string dst2)
        {
            return source.Source.ToString() == src2 && source.Destination.ToString() == dst2;
        }
        public static bool EndPointConv(this Artifact source, string src2, string dst2)
        {
            return (source.Source.ToString() == src2 && source.Destination.ToString() == dst2)
                ||
                    (source.Source.ToString() == dst2 && source.Destination.ToString() == src2);
        }
        /// <summary>
        /// Test if <paramref name="target"/> artifact occurs in window starting at <paramref name="source"/> artifact and the given <paramref name="interval"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="interval"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool After(this Artifact source, TimeSpan interval, Artifact target)
        {
            return target.Start > source.Start && target.Start < source.Start + interval;
        }
        public static bool Before(this Artifact source, TimeSpan interval, Artifact target)
        {
            return target.Start > source.Start - interval && target.Start < source.Start;
        }
        public static bool Window(this Artifact source, TimeSpan before, TimeSpan after, Artifact target)
        {
            return target.Start > source.Start -before && target.Start < source.Start + after;
        }


    }

}

// 0-Date first seen          
// 1-Duration 
// 2-Proto
// 3-Src IP Addr 
// 4-Src Pt                             
// 5-Dst IP Addr 
// 6-Dst Pt  
// 7-TLS Client version  
// 8-TLS Server version  
// 9-TLS Server cipher suite            
// 10-TLS server name (SNI)    
// 11-TLS Subject organisation name  
// 12-Packets    
// 13-Bytes
