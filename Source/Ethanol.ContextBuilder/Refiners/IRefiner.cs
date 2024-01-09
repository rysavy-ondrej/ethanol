namespace Ethanol.ContextBuilder.Refiners
{
    public interface IRefiner<T, R>
    {
        R? Refine(T item);
    }
}
