using Ethanol.ContextBuilder.Context;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Net;
using System.Net.Sockets;

namespace Ethanol.ContextBuilder.Enrichers
{
    public record FlowTag(DateTime StartTime, DateTime EndTime, ProtocolType Protocol, IPAddress LocalAddress, ushort LocalPort, IPAddress RemoteAddress, ushort RemotePort, string ProcessName)
    {
        public FlowKey GetFlowKey() => new FlowKey(Protocol, LocalAddress, LocalPort, RemoteAddress, RemotePort); 
    }
}
