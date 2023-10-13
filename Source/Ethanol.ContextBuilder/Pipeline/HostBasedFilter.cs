using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using System;
using System.Net;

namespace Ethanol.ContextBuilder.Pipeline
{
    public class HostBasedFilter
    {
        Func<IPAddress, bool> AddressFilter;

        public HostBasedFilter()
        {
            AddressFilter = x => true;
        }

        public HostBasedFilter(Func<IPAddress, bool> value)
        {
            this.AddressFilter = value;
        }

        public static HostBasedFilter FromHostPrefix(IPAddressPrefix targetHostPrefix)
        {
            if (targetHostPrefix == null)
            {
                return new HostBasedFilter();
            }
            else
            {
                return new HostBasedFilter(x => targetHostPrefix.Match(x));
            }
        }

        public bool Match(IPAddress address)
        {
            return AddressFilter(address);
        }

        public bool Evaluate(ObservableEvent<IpHostContext> evt)
        {
            return Match(evt.Payload.HostAddress);
        }
    }
}
