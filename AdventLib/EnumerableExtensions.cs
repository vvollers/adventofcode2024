namespace adventofcode.AdventLib;

using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions
{
    /// <summary>
    ///     Returns a list of indexes for a range of 0 -> max(list) where there is no element for that index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to be processed.</param>
    /// <param name="getterFunc">A function that returns a property of an element of the list.</param>
    /// <returns>An IEnumerable of integers representing the available indexes in the list.</returns>
    public static IEnumerable<int> AvailableIndexInList<T>(this List<T> list, Func<T, int> getterFunc)
    {
        return Enumerable.Range(0, list.Max(getterFunc)).Where(x => list.All(g => getterFunc(g) != x));
    }

    /// <summary>
    ///     Generates a pair for every unique combination in a list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to be processed.</param>
    /// <returns>An IEnumerable of tuples, where each tuple contains a pair of elements from the original list.</returns>
    public static IEnumerable<(T, T)> GenerateCombinations<T>(this List<T> list)
    {
        return Enumerable.Range(0, list.Count).
            Select(i => Enumerable.Range(i + 1, list.Count - i - 1).Select(j => (list[i], list[j]))).
            SelectMany(x => x);
    }


    /// <summary>
    ///     Repeats the source for a specified count and returns a string.
    /// </summary>
    /// <typeparam name="T">The type of the source.</typeparam>
    /// <param name="source">The source to be repeated.</param>
    /// <param name="count">The number of times to repeat the source.</param>
    /// <returns>A string that repeats the source for the specified count.</returns>
    public static string Repeat<T>(this T source, int count)
    {
        return string.Concat(Enumerable.Repeat(source, count));
    }

    /// <summary>
    ///     Replaces an element in the list that satisfies the predicate with a new value, or adds the new value to the list if
    ///     no element satisfies the predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="source">The list to be processed.</param>
    /// <param name="predicate">A function that defines the condition to be met.</param>
    /// <param name="with">The new value to replace the existing value or to be added to the list.</param>
    /// <returns>The index of the replaced or added value.</returns>
    public static int ReplaceOrAdd<T>(this List<T> source, Func<T, bool> predicate, T with)
    {
        if (TryGetIndex(source, predicate, out int index))
        {
            source[index] = with;
            return index;
        }

        source.Add(with);
        return source.Count - 1;
    }

    public static T2 SelectList<T, T2>(this List<T> source, Func<List<T>, T2> selector)
    {
        return selector(source);
    }

    public static List<IEnumerable<T>> SplitOn<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var sublist = new List<T>();
        var result = new List<IEnumerable<T>>();
        foreach (T item in source)
        {
            if (predicate(item))
            {
                if (sublist.Any())
                {
                    result.Add(sublist);
                    sublist = new List<T>();
                }
            }
            else
            {
                sublist.Add(item);
            }
        }

        if (sublist.Any())
        {
            result.Add(sublist);
        }

        return result;
    }

    /// <summary>
    ///     Splits the source into sublists when the predicate is met.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source.</typeparam>
    /// <param name="source">The source to be processed.</param>
    /// <param name="predicate">A function that defines the condition to be met.</param>
    /// <returns>An IEnumerable of sublists.</returns>
    public static IEnumerable<IEnumerable<T>> SplitWhen<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var sublist = new List<T>();
        foreach (T item in source)
        {
            if (predicate(item))
            {
                if (sublist.Any())
                {
                    yield return sublist;
                    sublist = new List<T>();
                }
            }
            else
            {
                sublist.Add(item);
            }
        }

        if (sublist.Any())
        {
            yield return sublist;
        }
    }

    /// <summary>
    ///     Tries to get the index of a value from the source that satisfies the predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source.</typeparam>
    /// <param name="source">The source to be processed.</param>
    /// <param name="predicate">A function that defines the condition to be met.</param>
    /// <param name="index">The index of the value that satisfies the predicate.</param>
    /// <returns>A boolean indicating whether an index was found.</returns>
    public static bool TryGetIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate, out int index)
    {
        int i = 0;
        foreach (T item in source)
        {
            if (predicate(item))
            {
                index = i;
                return true;
            }

            i++;
        }

        index = -1;
        return false;
    }

    /// <summary>
    ///     Extension method for the List<T> type. It pairs up elements from the list into tuples.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="source">The list to be processed.</param>
    /// <returns>A list of tuples, where each tuple contains a pair of elements from the original list.</returns>
    public static List<(T first, T second)> ZipGrid<T>(this List<T> source)
    {
        // Take the first half of the list
        return source.Take(source.Count / 2)
            // Pair up these elements with the corresponding elements in the second half of the list
            .
            Zip(source.Skip(source.Count / 2), (first, second) => (first, second))
            // Convert the resulting enumerable of tuples back into a list
            .
            ToList();
    }
}
