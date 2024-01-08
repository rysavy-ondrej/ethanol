namespace Ethanol.ContextBuilder.Polishers
{
    public interface IRefiner<T, R>
    {
        R? Refine(T item);
    }
}
