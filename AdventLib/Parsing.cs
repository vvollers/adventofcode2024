namespace adventofcode.AdventLib;

using System;
using System.Collections.Generic;
using System.Linq;

public static class Parsing
{
    public static void ActionOn<IEquatable>(this IEquatable c, IEquatable mustbe, Action action)
    {
        if (c.Equals(mustbe))
        {
            action.Invoke();
        }
    }

    public static void ActionOn<IEquatable>(this IEquatable c, Func<IEquatable, bool> predicate, Action action)
    {
        if (predicate(c))
        {
            action.Invoke();
        }
    }

    public static void ActionOn<IEquatable>(this IEquatable c, params (IEquatable mustbe, Action action)[] prms)
    {
        foreach ((IEquatable mustbe, Action action) in prms)
        {
            if (c.Equals(mustbe))
            {
                action.Invoke();
            }
        }
    }

    public static void ActionOn<IEquatable>(
        this IEquatable c,
        params (Func<IEquatable, bool> predicate, Action action)[] prms
    )
    {
        foreach ((Func<IEquatable, bool> predicate, Action action) in prms)
        {
            if (predicate(c))
            {
                action.Invoke();
            }
        }
    }


    /// <summary>
    ///     Performs an action on each character in the input string that matches a specified condition.
    /// </summary>
    /// <param name="input">The input string to be processed.</param>
    /// <param name="match">A function that defines the condition to be met.</param>
    /// <param name="action">The action to be performed on each matching character.</param>
    public static void ActionOnCharacterMatch(string input, Func<char, bool> match, Action<char, int, int> action)
    {
        ActionOnCharacterMatch(input, (c, _, _) => match(c), action);
    }

    /// <summary>
    ///     Performs an action on each character in the input string that matches a specified condition.
    /// </summary>
    /// <param name="input">The input string to be processed.</param>
    /// <param name="match">A function that defines the condition to be met.</param>
    /// <param name="action">The action to be performed on each matching character.</param>
    public static void ActionOnCharacterMatch(string input, Func<char, bool> match, Action<int, int> action)
    {
        ActionOnCharacterMatch(input, (c, _, _) => match(c), (_, x, y) => action(x, y));
    }

    /// <summary>
    ///     Performs an action on each character in the input string that matches a specified character.
    /// </summary>
    /// <param name="input">The input string to be processed.</param>
    /// <param name="matchChar">The character to be matched.</param>
    /// <param name="action">The action to be performed on each matching character.</param>
    public static void ActionOnCharacterMatch(string input, char matchChar, Action<int, int> action)
    {
        ActionOnCharacterMatch(input, (c, _, _) => c == matchChar, (_, x, y) => action(x, y));
    }

    /// <summary>
    ///     Performs an action on each character in the input string that matches a specified character.
    /// </summary>
    /// <param name="input">The input string to be processed.</param>
    /// <param name="matchChar">The character to be matched.</param>
    /// <param name="action">The action to be performed on each matching character.</param>
    public static void ActionOnCharacterMatch(string input, char matchChar, Action<char, int, int> action)
    {
        ActionOnCharacterMatch(input, (c, _, _) => c == matchChar, action);
    }

    /// <summary>
    ///     Performs an action on each character in the input string that matches a specified condition.
    /// </summary>
    /// <param name="input">The input string to be processed.</param>
    /// <param name="match">A function that defines the condition to be met.</param>
    /// <param name="action">The action to be performed on each matching character.</param>
    public static void ActionOnCharacterMatch(
        string input,
        Func<char, int, int, bool> match,
        Action<char, int, int> action
    )
    {
        input.Split("\n").
            Select((line, y) => (line, y)).
            ToList().
            ForEach(
                l => Enumerable.Select<char, (char c, int x)>(l.line.ToCharArray(), (c, x) => (c, x)).
                    Where(r => match(r.c, r.x, l.y)).
                    ToList().
                    ForEach(r => action(r.c, r.x, l.y))
            );
    }

    public static void DebugPrint<T>(this List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Console.WriteLine($"#{i}: {list[i]}");
        }
    }

    public static void DebugPrint<T>(this List<T> list, Func<T, string> toString)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Console.WriteLine($"#{i}: {toString(list[i])}");
        }
    }

    public static TEnum FromChar<TEnum>(char charValue, TEnum defaultValue)
        where TEnum : struct, IConvertible
    {
        if (Enum.IsDefined(typeof(TEnum), (int)charValue))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), (int)charValue);
        }

        return defaultValue;
    }

    /// <summary>
    ///     Parses line data from the input string and builds a list of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of elements to be built.</typeparam>
    /// <param name="input">The input string to be processed.</param>
    /// <param name="match">A function that defines the condition to be met.</param>
    /// <param name="builder">A function that builds an element of type T.</param>
    /// <returns>A list of elements of type T.</returns>
    public static List<T> ParseLineData<T>(string input, Func<char, bool> match, Func<char, int, int, T> builder)
    {
        return ParseLineData(input, (c, _, _) => match(c), builder);
    }

    /// <summary>
    ///     Parses line data from the input string and builds a list of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of elements to be built.</typeparam>
    /// <param name="input">The input string to be processed.</param>
    /// <param name="matchChar">The character to be matched.</param>
    /// <param name="builder">A function that builds an element of type T.</param>
    /// <returns>A list of elements of type T.</returns>
    public static List<T> ParseLineData<T>(string input, char matchChar, Func<char, int, int, T> builder)
    {
        return ParseLineData(input, (c, _, _) => c == matchChar, builder);
    }

    /// <summary>
    ///     Parses line data from the input string and builds a list of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of elements to be built.</typeparam>
    /// <param name="input">The input string to be processed.</param>
    /// <param name="match">A function that defines the condition to be met.</param>
    /// <param name="builder">A function that builds an element of type T.</param>
    /// <returns>A list of elements of type T.</returns>
    public static List<T> ParseLineData<T>(string input, Func<char, bool> match, Func<int, int, T> builder)
    {
        return ParseLineData(input, (c, _, _) => match(c), (_, x, y) => builder(x, y));
    }

    /// <summary>
    ///     Parses line data from the input string and builds a list of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of elements to be built.</typeparam>
    /// <param name="input">The input string to be processed.</param>
    /// <param name="matchChar">The character to be matched.</param>
    /// <param name="builder">A function that builds an element of type T.</param>
    /// <returns>A list of elements of type T.</returns>
    public static List<T> ParseLineData<T>(string input, char matchChar, Func<int, int, T> builder)
    {
        return ParseLineData(input, (c, _, _) => matchChar == c, (_, x, y) => builder(x, y));
    }

    /// <summary>
    ///     Parses line data from the input string and builds a list of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of elements to be built.</typeparam>
    /// <param name="input">The input string to be processed.</param>
    /// <param name="match">A function that defines the condition to be met.</param>
    /// <param name="builder">A function that builds an element of type T.</param>
    /// <returns>A list of elements of type T.</returns>
    public static List<T> ParseLineData<T>(
        string input,
        Func<char, int, int, bool> match,
        Func<char, int, int, T> builder
    )
    {
        return input.Split("\n").
            Select(
                (line, y) =>
                    line.ToCharArray().
                        Select((c, x) => (c, x)).
                        Where(r => match(r.c, r.x, y)).
                        Select(r => builder(r.c, r.x, y))
            ).
            SelectMany(x => x).
            ToList();
    }

    public static List<T> ParseLineData<T>(this string input, Func<string, T> builder)
    {
        return input.Split("\n").Select(builder).ToList();
    }

    public static (List<T1>, List<T2>) ParseLineData<T1, T2>(
        this string input,
        Func<string, T1> builder,
        Func<string, T2> builder2
    )
    {
        return input.Split("\n").
            SplitOn(line => line.Length == 0).
            SelectList(parts => (parts[0].Select(builder).ToList(), parts[1].Select(builder2).ToList()));
    }

    public static byte[][] ParseToByteGrid(this string input)
    {
        return input.Split("\n").Select(o => o.ToCharArray().Select(j => byte.Parse("" + j)).ToArray()).ToArray();
    }

    /// <summary>
    ///     Parses the input string into a 2D array of characters.
    /// </summary>
    /// <param name="input">The input string to be processed.</param>
    /// <returns>A 2D array of characters.</returns>
    public static char[][] ParseToCharGrid(this string input)
    {
        return input.Split("\n").Select(o => o.Trim().ToCharArray()).ToArray();
    }
}
