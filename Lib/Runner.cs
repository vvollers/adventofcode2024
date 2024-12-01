namespace AdventOfCode;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate
)]
internal class ProblemNameAttribute(string name) : Attribute
{
    public readonly string Name = name;
}

internal interface ISolver
{
    object PartOne(string input);

    object PartTwo(string input)
    {
        return null;
    }
}

internal static class SolverExtensions
{
    public static int Day(this ISolver solver)
    {
        return Day(solver.GetType());
    }

    public static int Day(Type t)
    {
        return int.Parse(t.FullName.Split('.')[2].Substring(3));
    }

    public static string DayName(this ISolver solver)
    {
        return $"Day {solver.Day()}";
    }

    public static string GetName(this ISolver solver)
    {
        return (solver.GetType().GetCustomAttribute(typeof(ProblemNameAttribute)) as ProblemNameAttribute).Name;
    }

    public static int Sloc(this ISolver solver)
    {
        string file = solver.WorkingDir() + "/Solution.cs";
        if (File.Exists(file))
        {
            string solution = File.ReadAllText(file);
            return Regex.Matches(solution, @"\n").Count;
        }

        return -1;
    }

    public static IEnumerable<object> Solve(this ISolver solver, string input)
    {
        yield return solver.PartOne(input);
        object res = solver.PartTwo(input);
        if (res != null)
        {
            yield return res;
        }
    }

    public static ISplashScreen SplashScreen(this ISolver solver)
    {
        Type tsplashScreen = Assembly.GetEntryAssembly().
            GetTypes().
            Where(t => t.GetTypeInfo().IsClass && typeof(ISplashScreen).IsAssignableFrom(t)).
            Single(t => Year(t) == solver.Year());
        return (ISplashScreen)Activator.CreateInstance(tsplashScreen);
    }

    public static string WorkingDir(int year)
    {
        return Path.Combine(year.ToString());
    }

    public static string WorkingDir(int year, int day)
    {
        return Path.Combine(WorkingDir(year), "Day" + day.ToString("00"));
    }

    public static string WorkingDir(this ISolver solver)
    {
        return WorkingDir(solver.Year(), solver.Day());
    }

    public static int Year(this ISolver solver)
    {
        return Year(solver.GetType());
    }

    public static int Year(Type t)
    {
        return int.Parse(t.FullName.Split('.')[1].Substring(1));
    }
}

internal record SolverResult(string[] answers, string[] errors);

internal static class Runner
{
    public static void RunAll(params ISolver[] solvers)
    {
        if (solvers.Length == 0)
        {
            WriteLine(ConsoleColor.Yellow, "No solvers found. To start writing one, use");
            WriteLine(ConsoleColor.Yellow, "> dotnet run update 20xx/xx");
            WriteLine();
            return;
        }

        var errors = new List<string>();

        int lastYear = -1;
        List<(int day, int sloc)> slocs = new();
        foreach (ISolver solver in solvers)
        {
            if (lastYear != solver.Year())
            {
                SlocChart.Show(slocs);
                slocs.Clear();

                solver.SplashScreen().Show();
                lastYear = solver.Year();
            }

            slocs.Add((solver.Day(), solver.Sloc()));
            SolverResult result = RunSolver(solver);
            WriteLine();
            errors.AddRange(result.errors);
        }

        SlocChart.Show(slocs);
        WriteLine();

        if (errors.Any())
        {
            WriteLine(ConsoleColor.Red, "Errors:\n" + string.Join("\n", errors));
        }

        WriteLine();
    }

    public static SolverResult RunSolver(ISolver solver)
    {
        string workingDir = solver.WorkingDir();
        string indent = "    ";
        Write(ConsoleColor.White, $"{indent}{solver.DayName()}: {solver.GetName()}");
        WriteLine();
        string file = Path.Combine(workingDir, "input.in");
        string refoutFile = file.Replace(".in", ".refout");
        string[] refout = File.Exists(refoutFile) ? File.ReadAllLines(refoutFile) : null;
        string input = GetNormalizedInput(file);
        int iline = 0;
        var answers = new List<string>();
        var errors = new List<string>();
        var stopwatch = Stopwatch.StartNew();
        foreach (object line in solver.Solve(input))
        {
            long ticks = stopwatch.ElapsedTicks;
            if (line is OcrString ocrString)
            {
                Console.WriteLine("\n" + ocrString.st.Indent(10, true));
            }

            answers.Add(line.ToString());

            ConsoleColor statusColor;
            string status;
            string err = null;
            if (refout == null || refout.Length <= iline)
            {
                statusColor = ConsoleColor.Cyan;
                status = "?";
            }
            else if (refout[iline] == line.ToString())
            {
                statusColor = ConsoleColor.DarkGreen;
                status = "✓";
            }
            else
            {
                statusColor = ConsoleColor.Red;
                status = "X";
                err = $"{solver.DayName()}: In line {iline + 1} expected '{refout[iline]}' but found '{line}'";
            }

            if (err != null)
            {
                errors.Add(err);
            }

            Write(statusColor, $"{indent}  {status}");
            Console.Write($" {line} ");
            double diff = ticks * 1000.0 / Stopwatch.Frequency;

            if (diff > 1000)
            {
                WriteLine(ConsoleColor.Red, $"({diff:F3} ms)");
            }
            else if (diff > 500)
            {
                WriteLine(ConsoleColor.Yellow, $"({diff:F3} ms)");
            }
            else
            {
                WriteLine(ConsoleColor.Green, $"({diff:F3} ms)");
            }

            iline++;
            stopwatch.Restart();
        }

        return new SolverResult(answers.ToArray(), errors.ToArray());
    }

    private static string GetNormalizedInput(string file)
    {
        string input = File.ReadAllText(file);

        // on Windows we have "\r\n", not sure if this causes more harm or not
        input = input.Replace("\r", "");

        if (input.EndsWith('\n'))
        {
            input = input.Substring(0, input.Length - 1);
        }

        return input;
    }

    private static void Write(ConsoleColor color = ConsoleColor.Gray, string text = "")
    {
        ConsoleColor c = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = c;
    }

    private static void WriteLine(ConsoleColor color = ConsoleColor.Gray, string text = "")
    {
        Write(color, text + "\n");
    }
}
