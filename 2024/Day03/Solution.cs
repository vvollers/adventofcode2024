namespace AdventOfCode.Y2024.Day03;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

[ProblemName("Mull It Over")]
internal class Solution : ISolver
{
    private readonly string testData = "xmul(2,4)%&mul[3,7]!@^do_not_mul(5,5)+mul(32,64]then(mul(11,8)mul(8,5))";
    private readonly string testData2 = "xmul(2,4)&mul[3,7]!^don't()_mul(5,5)+mul(32,64](mul(11,8)undo()?mul(8,5))";

    public IEnumerable<(int, int)> GetMatches(string data)
    {
        string pattern = @"mul\((\d+),(\d+)\)";
        MatchCollection matches = Regex.Matches(data, pattern);
        var sb = new StringBuilder();
        foreach (Match match in matches)
        {
            int x = int.Parse(match.Groups[1].Value);
            int y = int.Parse(match.Groups[2].Value);
            yield return (x, y);
        }
    }

    public IEnumerable<(string inst, int x, int y)> GetMatches2(string data)
    {
        string pattern = @"(mul|do|don\'t)(\((\d+),(\d+)\)|\(\))";

        MatchCollection matches = Regex.Matches(data, pattern);
        var sb = new StringBuilder();
        foreach (Match match in matches)
        {
            string instruction = match.Groups[1].Value;
            if (instruction == "mul")
            {
                int x = int.Parse(match.Groups[3].Value);
                int y = int.Parse(match.Groups[4].Value);

                yield return (instruction, x, y);
            }
            else
            {
                yield return (instruction, 0, 0);
            }
        }
    }

    public object PartOne(string input)
    {
        return this.GetMatches(input).Sum(item => item.Item1 * item.Item2);
    }

    public object PartTwo(string input)
    {
        bool enabled = true;
        int total = 0;
        foreach ((string inst, int x, int y) item in this.GetMatches2(input))
        {
            if (item.inst == "do")
            {
                enabled = true;
            }
            else if (item.inst == "don't")
            {
                enabled = false;
            }
            else if (enabled)
            {
                total += item.x * item.y;
            }
        }

        return total;
    }
}
