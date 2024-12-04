namespace AdventOfCode.Y2024.Day04;

using System.Collections.Generic;
using System.Linq;
using adventofcode.AdventLib;

[ProblemName("Ceres Search")]
internal class Solution : ISolver
{
    private readonly List<List<(int xx, int yy)>> directions =
    [
        [(0, 0), (1, 0), (2, 0), (3, 0),], // horizontal ->
        [(0, 0), (-1, 0), (-2, 0), (-3, 0),], // horizontal <-
        [(0, 0), (0, 1), (0, 2), (0, 3),], // vertical ->
        [(0, 0), (0, -1), (0, -2), (0, -3),], // vertical <-
        [(0, 0), (1, 1), (2, 2), (3, 3),], // diagonal XY ->
        [(0, 0), (-1, -1), (-2, -2), (-3, -3),], // diagonal XY <-
        [(0, 0), (1, -1), (2, -2), (3, -3),], // diagonal YX ->
        [(0, 0), (-1, 1), (-2, 2), (-3, 3),], // diagonal YX <-
    ];

    private readonly string testInput = """
                                        MMMSXXMASM
                                        MSAMXMSMSA
                                        AMXSXMAAMM
                                        MSAMASMSMX
                                        XMASAMXAMM
                                        XXAMMXXAMA
                                        SMSMSASXSS
                                        SAXAMASAAA
                                        MAMMMXMMMM
                                        MXMXAXMASX
                                        """;

    /*
     *   --- --- ---
     *   --- --- ---
     *   --- --- ---
     *   --- -o- ---
     *   --- --- ---
     *   --- --- ---
     *   --- --- ---
     */

    public object PartOne(string input)
    {
        string searchFor = "XMAS";
        var field = new CharField(input.ParseToCharGrid());
        int totalCount = 0;

        for (int y = 0; y < field.Height; y++)
        {
            for (int x = 0; x < field.Width; x++)
            {
                totalCount += this.directions.Count(
                    dir => searchFor.Select(
                            (c, i) =>
                            {
                                (int dx, int dy) = dir[i];
                                int nx = x + dx, ny = y + dy;
                                return field.IsInside(nx, ny) && field[nx, ny] == c;
                            }
                        ).
                        All(valid => valid)
                );
            }
        }

        return totalCount;
    }

    public object PartTwo(string input)
    {
        var field = new CharField(input.ParseToCharGrid());
        int totalCount = 0;
        field.ApplyToAnyCharacter(
            (c, _, _) => c == 'A',
            (_, x, y) =>
            {
                if (x < 1 || y < 1 || x > field.Width - 2 || y > field.Height - 2)
                {
                    return;
                }

                bool Chk(int x, int y, char c)
                {
                    return field[x, y] == c;
                }

                if (((Chk(x - 1, y - 1, 'M') && Chk(x + 1, y + 1, 'S')) ||
                     (Chk(x + 1, y + 1, 'M') && Chk(x - 1, y - 1, 'S'))) &&
                    ((Chk(x - 1, y + 1, 'M') && Chk(x + 1, y - 1, 'S')) ||
                     (Chk(x + 1, y - 1, 'M') && Chk(x - 1, y + 1, 'S'))))
                {
                    totalCount++;
                }
            }
        );
        return totalCount;
    }
}
