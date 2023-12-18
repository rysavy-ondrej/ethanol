
namespace Ethanol.DataObjects
{
    /// <summary>
    /// Represents an attribute within a context, containing its name, key, value, and a measure of its reliability.
    /// </summary>
    /// <remarks>
    /// The <see cref="ContextAttribute"/> record is designed to encapsulate specific details or properties within a given context.
    /// Each attribute has a key and name associated with it, a value that can be of any type, and a reliability measure 
    /// indicating the confidence or accuracy of the attribute's value.
    /// </remarks>
    /// <param name="Name">The descriptive name of the attribute.</param>
    /// <param name="Key">The unique identifier key for the attribute.</param>
    /// <param name="Value">The actual value of the attribute. Can be of any object type.</param>
    /// <param name="Reliability">A measure of the reliability or confidence in the attribute's value, typically ranging from 0.0 (not reliable) to 1.0 (completely reliable).</param>
    public record ContextAttribute(string Name, string Key, object Value, float Reliability);


}
