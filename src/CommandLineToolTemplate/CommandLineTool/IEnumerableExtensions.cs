using System.Diagnostics.CodeAnalysis;

namespace CommandLineTools;

public static class IEnumerableExtensions
{
    /// <summary>
    /// Executes action for each item in collection.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source) action(item);
    }

    /// <summary>
    /// Returns true if collection is not null and contains elements.
    /// </summary>
    public static bool IsAny<T>([NotNullWhen(true)] this IEnumerable<T>? source) => source?.Any() == true;

    /// <summary>
    /// Returns true if collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source) => source?.Any() != true;

    /// <summary>
    /// Returns collection of item results that are not null.
    /// </summary>
    public static IEnumerable<TResult> SelectNotNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        List<TResult> results = new List<TResult>();

        foreach (TSource item in source.ToArray()) if (selector(item) is { } notNull) results.Add(notNull);

        return results;
    }
}
