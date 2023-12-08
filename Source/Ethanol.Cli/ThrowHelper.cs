public static class ThrowHelper
{
    public static void ThrowIfNull<T>(this T? obj, string name) where T : class
    {
        if (obj == null)
        {
            throw new ArgumentNullException(name);
        }
    }
    public static void ElseThrow(this bool condition, Func<Exception> exceptionFactory)
    {
        if (!condition)
        {
            throw exceptionFactory();
        }
    }
}
