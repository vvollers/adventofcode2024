namespace AdventOfCode.Y2024.Day02;

using System.Collections.Generic;
using System.Linq;
using adventofcode.AdventLib;

internal enum Slope
{
    Up,
    Down,
}

[ProblemName("Red-Nosed Reports")]
internal class Solution : ISolver
{
    public object PartOne(string input)
    {
        List<List<int>> inputLines = input.ParseLineData(line => line.Split(" ").Select(int.Parse).ToList());
        List<bool> linesIsSafe = inputLines.ConvertAll(a => this.IsSafe(a));

        return linesIsSafe.Count(b => b);
    }

    public object PartTwo(string input)
    {
        List<List<int>> inputLines = input.ParseLineData(line => line.Split(" ").Select(int.Parse).ToList());
        List<bool> linesIsSafe = inputLines.ConvertAll(this.IsSafeAny);

        return linesIsSafe.Count(b => b);
    }

    private int GetDiff(int a, int b)
    {
        return a > b ? a - b : b - a;
    }

    private Slope GetSlope(int a, int b)
    {
        return a > b ? Slope.Down : Slope.Up;
    }


    private bool IsSafe(List<int> arg, int minusIndex = -1)
    {
        List<int> pool = minusIndex == -1 ? arg : arg.Where((_, i) => i != minusIndex).ToList();
        if (pool[0] == pool[1])
        {
            return false;
        }

        Slope slope = this.GetSlope(pool[0], pool[1]);

        int previousData = pool[0];
        for (int i = 1; i < pool.Count; i++)
        {
            int currentData = pool[i];

            if (!this.IsSafePart(slope, previousData, currentData))
            {
                return false;
            }

            previousData = currentData;
        }

        return true;
    }

    private bool IsSafeAny(List<int> arg)
    {
        for (int i = 0; i < arg.Count; i++)
        {
            if (this.IsSafe(arg, i))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsSafePart(Slope slope, int previousData, int currentData)
    {
        if (this.GetSlope(previousData, currentData) != slope)
        {
            return false;
        }

        int diff = this.GetDiff(previousData, currentData);

        return diff is >= 1 and <= 3;
    }
}
