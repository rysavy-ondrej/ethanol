using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;

namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// Represents a TagObject, an essential information element within a context.
    /// TagObjects provide a standardized way to annotate or categorize data. 
    /// </summary>
    /// <remarks>
    /// Each TagObject comprises:
    /// <list type="bullet">
    /// <item> Type: The category or classification of the tag.</item>
    /// <item> Key: A unique identifier that specifies what the tag pertains to.</item>
    /// <item> Value: The actual data or information associated with the key.</item>
    /// </list>
    /// 
    /// Furthermore, to ensure data integrity and relevancy:
    /// <list type="bullet">
    /// <item> Reliability: Each tag carries a reliability score, giving an indication of the confidence or accuracy of the data.</item>
    /// <item> Validity Range: Tags have start and end times, demarcating the time period during which the information is considered relevant or accurate.</item>
    /// </list>
    /// To cater for diverse data needs, tags can also encapsulate additional granular details, enhancing the richness of the context information provided.
    /// </remarks>
    public class TagObject
    {
        /// <summary>
        /// Gets or sets the category or classification of the tag. This provides a high-level 
        /// categorization to understand the kind or domain of the tag.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the tag within its type. This key 
        /// allows for easy identification and differentiation of tags.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the descriptive information or data associated with the tag. This 
        /// value typically elaborates on the nature or specifics of the tag.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the score indicating the trustworthiness or confidence level 
        /// of the tag. A higher reliability indicates greater confidence in the accuracy 
        /// and relevancy of the tag.
        /// </summary>
        public double Reliability { get; set; }
        /// <summary>
        /// Gets or sets the starting timestamp indicating when the tag first becomes valid.
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Gets or sets the ending timestamp indicating when the tag's validity expires.
        /// Together with <see cref="StartTime"/>, this forms the validity range for the tag.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the granular or extended information related to the tag, 
        /// typically stored as a serialized JSON string. This can include any extra 
        /// attributes, metadata, or deeper context about the tag.
        /// </summary>
        public dynamic Details { get; set; }

        /// <summary>
        /// Retrieves the details of the tag object as a specified type.
        /// </summary>
        /// <typeparam name="T">The type to which the details should be converted.</typeparam>
        /// <returns>The details of the tag object as the specified type.</returns>
        /// <remarks>
        /// If the specified type <typeparamref name="T"/> matches the current type of Details, 
        /// it returns the details directly. Otherwise, it serializes the Details into JSON 
        /// and then deserializes it into the specified type.
        /// </remarks>
        internal T GetDetailsAs<T>()
        {
            if (typeof(T) == Details.GetType())
            {
                return (T)Details;
            }
            else
            {
                var json = JsonSerializer.Serialize(Details);
                return JsonSerializer.Deserialize<T>(json);
            }
        }
    }

}
