
namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents an Internet Service tag within a context, containing its name and a measure of its reliability.
    /// </summary>
    /// <param name="Name">The Internet services name as given by Netify database.</param>
    /// <param name="Reliability">The reliability of the informaiton.</param>
    public record InternetServiceTag(string Name, float Reliability);


}
