using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Enrichers;
using Ethanol.ContextBuilder.Observable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;


/// <summary>
/// Provides tags associated with remote hosts for a specified time range using a <see cref="ITagDataSource{T}"/>.
/// </summary>
/// <typeparam name="TagObject">The type of the tag object.</typeparam>
/// <typeparam name="IpHostContext">The type of the IP host context.</typeparam>
public class NetifyTagProvider : ITagDataProvider<TagObject, IpHostContext>
{
    /// <summary>
    /// The data source for retrieving tags.
    /// </summary>
    private readonly ITagDataSource<TagObject> _tagDataSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetifyTagProvider"/> class.
    /// </summary>
    /// <param name="tagDataSource">The tag data source.</param>
    public NetifyTagProvider(ITagDataSource<TagObject> tagDataSource)
    {
        _tagDataSource = tagDataSource ?? throw new ArgumentNullException(nameof(tagDataSource));
    }

    /// <summary>
    /// Retrieves the tags associated with the provided <see cref="IpHostContext"/>.
    /// </summary>
    /// <param name="value">The observable event containing the IP host context.</param>
    /// <returns>The tags associated with the IP host context.</returns>
    public IEnumerable<TagObject> GetTags(ObservableEvent<IpHostContext> value)
    {
        if (value.Payload == null || value.Payload.HostAddress == null || value.Payload.Flows == null || value.Payload.Flows.Length == 0)
            return Enumerable.Empty<TagObject>();

        var host = value.Payload.HostAddress;
        var start = value.StartTime;
        var end = value.EndTime;
        var flows = value.Payload.Flows;
        var remoteHosts = flows.Select(x => x.GetRemoteAddress(host)).Where(x=> x!= null).Distinct();
        return GetRemoteTags(remoteHosts!, start, end);
    }

    /// <summary>
    /// Retrieves tags associated with remote hosts for a specified time range.
    /// </summary>
    /// <remarks>
    /// This method takes an array of IpFlow objects and two DateTime values, and returns a dictionary
    /// where the key is a string representation of a remote address (from the IpFlow array), and the
    /// value is an array of NetifyApplication objects that match the given remote address and time range.
    /// If there are no matching NetifyApplication objects, an empty array is returned.
    /// </remarks>
    /// <param name="flows">The array of IpFlow objects.</param>
    /// <param name="start">The start time of the time range.</param>
    /// <param name="end">The end time of the time range.</param>
    /// <returns>A dictionary where the key is a string representation of a remote address (from the IpFlow array), and the
    /// value is an array of NetifyApplication objects that match the given remote address and time range.
    /// </returns>
    private IEnumerable<TagObject> GetRemoteTags(IEnumerable<IPAddress> remoteHosts, DateTime start, DateTime end)
    {
        //return remoteHosts.SelectMany(addr => _tagQueryable?.Get(addr.ToString(), nameof(NetifyTag), start, end) ?? Enumerable.Empty<TagObject>());
        return _tagDataSource.GetMany(remoteHosts.Select(x => x.ToString()), "NetifyIp", start, end) ?? Enumerable.Empty<TagObject>();
    }
}
