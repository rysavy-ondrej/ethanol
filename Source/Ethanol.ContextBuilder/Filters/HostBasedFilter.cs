using Ethanol.DataObjects;
using System;
using System.Linq;
using System.Net;

namespace Ethanol.ContextBuilder.Pipeline
{
    /// <summary>
    /// Provides a filter mechanism based on IP addresses of hosts.
    /// </summary>
    public class HostBasedFilter : IHostBasedFilter
    {
        /// <summary>
        /// The delegate to perform the actual filtering based on IP addresses.
        /// </summary>
        private Func<IPAddress, bool> AddressFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBasedFilter"/> class that matches all IP addresses.
        /// </summary>
        public HostBasedFilter()
        {
            AddressFilter = x => true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBasedFilter"/> class that matches all IP addresses.
        /// </summary>
        public HostBasedFilter(params IPAddressPrefix[] prefixes)
        {
            if (prefixes == null) throw new ArgumentException(nameof(prefixes));
            bool MatchPrefix(IPAddress ip)
            {
                return prefixes.Any(x => x.Match(ip));
            }
            AddressFilter = MatchPrefix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBasedFilter"/> class with a specified filter function.
        /// </summary>
        /// <param name="value">The filter function to be used for matching IP addresses.</param>
        public HostBasedFilter(Func<IPAddress, bool> value)
        {
            this.AddressFilter = value;
        }

        /// <summary>
        /// Creates a new <see cref="HostBasedFilter"/> instance using the specified host prefix.
        /// </summary>
        /// <param name="targetHostPrefix">The IP address prefix for the filter. If null, a filter that matches all addresses will be returned.</param>
        /// <returns>A new <see cref="HostBasedFilter"/> instance.</returns>
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

        /// <summary>
        /// Determines whether the provided IP address matches the filter criteria.
        /// </summary>
        /// <param name="address">The IP address to be evaluated.</param>
        /// <returns>True if the IP address matches; otherwise, false.</returns>
        public bool Match(IPAddress address)
        {
            return AddressFilter(address);
        }

        /// <summary>
        /// Evaluates the filter against the IP host address contained within the provided observable event.
        /// </summary>
        /// <param name="evt">The observable event containing the IP host context.</param>
        /// <returns>True if the IP host address matches the filter; otherwise, false.</returns>
        public bool Evaluate(TimeRange<IpHostContext> evt)
        {
            if (evt.Value?.HostAddress == null) return false;
            return Match(evt.Value.HostAddress);
        }
    }
}
