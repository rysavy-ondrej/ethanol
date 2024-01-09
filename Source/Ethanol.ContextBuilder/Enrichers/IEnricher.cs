namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents an enricher that enriches an item of type <typeparamref name="T"/> and returns a result of type <typeparamref name="R"/>.
    /// </summary>
    /// <typeparam name="T">The type of the item to be enriched.</typeparam>
    /// <typeparam name="R">The type of the result after enrichment.</typeparam>
    public interface IEnricher<T, R>
    {
        /// <summary>
        /// Enriches the specified item.
        /// </summary>
        /// <param name="item">The item to be enriched.</param>
        /// <returns>The result of the enrichment.</returns>
        R? Enrich(T item);
    }
}