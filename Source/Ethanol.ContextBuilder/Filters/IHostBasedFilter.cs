using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using System.Net;

namespace Ethanol.ContextBuilder.Pipeline
{
    public interface IHostBasedFilter
    {
        bool Evaluate(ObservableEvent<IpHostContext> evt);
        bool Match(IPAddress address);
    }
}