public static class SafeArrayAccessExtension
{
    public static T[] AsSafe<T>(this T[]? array)
    {
        return array ?? Array.Empty<T>();
    }   


    public static T[] AsSafe<T>(this T[]? array, params T[] defaultArray)
    {
        return array ?? defaultArray;
    }   

    public static T[] AsSafe<T>(this IEnumerable<T>? enumerable)
    {
        return enumerable?.ToArray() ?? Array.Empty<T>();
    }

    public static T[] AsSafe<T>(this IEnumerable<T>? enumerable, Func<T, bool> predicate)
    {
        return enumerable?.Where(predicate).ToArray() ?? Array.Empty<T>();
    }


}