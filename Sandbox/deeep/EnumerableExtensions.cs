
 public static class EnumerableExtensions
 {
    public static Tuple<IEnumerable<T1>, IEnumerable<T2>> Unzip<T, T1, T2>(this IEnumerable<T> source, Func<T,T1> proj1, Func<T,T2> proj2)
    {
        var firsts = source.Select(proj1);
        var seconds = source.Select(proj2);
        return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(firsts, seconds);
    }
 }