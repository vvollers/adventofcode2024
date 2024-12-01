namespace AdventOfCode.Y2024.Day01;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using adventofcode.AdventLib;

public record Pair(int Left, int Right);

[ProblemName("Historian Hysteria")]
internal partial class Solution : ISolver
{
    public object PartOne(string input)
    {
        (List<int> leftNumbers, List<int> rightNumbers) = ParseInput(input);

        List<int> distances = leftNumbers.Zip(rightNumbers).
            Select(p => Math.Max(p.Second, p.First) - Math.Min(p.Second, p.First)).
            ToList();
        return distances.Sum();
    }

    public object PartTwo(string input)
    {
        (List<int> leftNumbers, List<int> rightNumbers) = ParseInput(input);

        List<int> counts = leftNumbers.ConvertAll(n => rightNumbers.Count(num => num == n));
        List<int> aggregates = leftNumbers.Zip(counts).Select(p => p.First * p.Second).ToList();
        return aggregates.Sum();
    }

    private static (List<int> leftNumbers, List<int> rightNumbers) ParseInput(string input)
    {
        List<Pair> pairs = input.ParseLineData(ParseLine);

        List<int> leftNumbers = pairs.ConvertAll(p => p.Left);
        List<int> rightNumbers = pairs.ConvertAll(p => p.Right);

        leftNumbers.Sort();
        rightNumbers.Sort();

        return (leftNumbers, rightNumbers);
    }

    private static Pair ParseLine(string line)
    {
        Match match = TwoNumbersRegex().Match(line);
        return new Pair(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
    }

    [GeneratedRegex(@"(\d+)\s+(\d+)")]
    private static partial Regex TwoNumbersRegex();
}
