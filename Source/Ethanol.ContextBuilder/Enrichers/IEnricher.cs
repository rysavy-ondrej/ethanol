namespace Ethanol.ContextBuilder.Enrichers
{
    public interface IEnricher<T, R>
    {
        R Enrich(T item);
    }
}