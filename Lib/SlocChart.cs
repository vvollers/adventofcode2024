namespace AdventOfCode;

using System;
using System.Collections.Generic;
using System.Linq;

internal static class SlocChart
{
    public static void Show(List<(int day, int sloc)> slocs)
    {
        if (slocs.Count < 2)
        {
            return;
        }

        Console.WriteLine("  In code lines");
        Console.WriteLine("");

        string chars = "█▁▂▃▄▅▆▇";

        var columns = new List<List<ColoredString>>();

        int icol = 0;
        int prevSloc = -1;
        foreach ((int day, int sloc) sloc in slocs)
        {
            icol++;
            var col = new List<ColoredString>();
            int h = sloc.sloc;

            var color = ConsoleColor.DarkGray;
            if (h > 200)
            {
                color = ConsoleColor.Red;
            }
            else if (h > 100)
            {
                color = ConsoleColor.Yellow;
            }

            h /= 2;

            if (Math.Abs(prevSloc - sloc.sloc) > 20 || (prevSloc < 100 && sloc.sloc < 100))
            {
                string slocSt = sloc.sloc.ToString();
                col.Add(slocSt.WithColor(ConsoleColor.White));
            }

            prevSloc = sloc.sloc;
            if (h % chars.Length != 0)
            {
                char ch = chars[h % chars.Length];
                col.Add($"{ch}{ch}".WithColor(color));
                h -= h % chars.Length;
            }

            while (h >= 0)
            {
                char ch = chars[0];
                col.Add($"{ch}{ch}".WithColor(color));
                h -= chars.Length;
            }

            col.Add(sloc.day.ToString().PadLeft(2, ' ').WithColor(ConsoleColor.White));
            int w = 3;
            col = col.Select(r => r.st.PadLeft(w).WithColor(r.c)).ToList();
            columns.Add(col);
        }

        var rows = new List<List<ColoredString>>();
        int height = columns.Select(col => col.Count).Max();
        for (int irow = 0; irow < height; irow++)
        {
            var row = new List<ColoredString>();
            foreach (List<ColoredString> col in columns)
            {
                ConsoleColor color = col.Count > irow ? col[col.Count - irow - 1].c : ConsoleColor.Gray;
                string st = col.Count > irow ? col[col.Count - irow - 1].st : "";
                int w = col.Select(r => r.st.Length).Max();
                st = st.PadLeft(w);
                row.Add(st.WithColor(color));
            }

            rows.Insert(0, row);
        }

        ConsoleColor c = Console.ForegroundColor;
        foreach (List<ColoredString> row in rows)
        {
            foreach (ColoredString item in row)
            {
                Console.ForegroundColor = item.c;
                Console.Write(item.st);
            }

            Console.WriteLine();
        }

        Console.ForegroundColor = c;
        Console.WriteLine("");
    }
}
