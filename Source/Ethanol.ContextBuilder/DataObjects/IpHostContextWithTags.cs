namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Represents a specialized context for a host in an IP network, enriched with raw tag data.
    /// </summary>
    /// <remarks>
    /// The <see cref="IpHostContextWithTags"/> class inherits from <see cref="IpHostContext{T}"/>, 
    /// using an array of <see cref="TagObject"/> as its generic parameter. This means that, in addition 
    /// to the properties inherited from the base class (which represent the IP host context), this 
    /// class is specifically designed to hold raw tag data associated with the IP host.
    /// </remarks>
    public class IpHostContextWithTags : IpHostContext<TagObject[]> { }
}
