using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AdventOfCode;

Type[] tsolvers = Assembly.GetEntryAssembly()!.GetTypes().
    Where(t => t.GetTypeInfo().IsClass && typeof(ISolver).IsAssignableFrom(t)).
    OrderBy(t => t.FullName).
    ToArray();

string usage = """
               Usage: dotnet run [arguments]
               1) To run the solutions and admire your advent calendar:
               
                [year]/[day|all]      Solve the specified problems
                today                 Shortcut to the above
                [year]                Solve the whole year
                all                   Solve everything
               
                calendars             Show the calendars

               2) To start working on new problems:
               login to https://adventofcode.com, then copy your session cookie, and export 
               it in your console like this
               
                export SESSION=73a37e9a72a...

               then run the app with
               
                update [year]/[day]   Prepares a folder for the given day, updates the input,
                                      the readme and creates a solution template.
                update today          Shortcut to the above.

               3) To upload your answer:
               set up your SESSION variable as above.
               
                upload [year]/[day]   Upload the answer for the selected year and day.
                upload today          Shortcut to the above.

               4) Don't forget to tip the maintainer https://github.com/sponsors/encse.
               """;

Action action = Command(
                    args,
                    Args("update", "([0-9]+)/([0-9]+)"),
                    m =>
                    {
                        int year = int.Parse(m[1]);
                        int day = int.Parse(m[2]);
                        return () => Updater.Update(year, day).Wait();
                    }
                ) ??
                Command(
                    args,
                    Args("update", "today"),
                    m =>
                    {
                        DateTime dt = DateTime.UtcNow.AddHours(-5);
                        if (dt is { Month: 12, Day: >= 1 and <= 25, })
                        {
                            return () => Updater.Update(dt.Year, dt.Day).Wait();
                        }

                        throw new AocCommuncationException("Event is not active. This option works in Dec 1-25 only)");
                    }
                ) ??
                Command(
                    args,
                    Args("upload", "([0-9]+)/([0-9]+)"),
                    m =>
                    {
                        int year = int.Parse(m[1]);
                        int day = int.Parse(m[2]);
                        return () =>
                        {
                            Type tsolver = tsolvers.First(
                                tsolver => SolverExtensions.Year(tsolver) == year &&
                                           SolverExtensions.Day(tsolver) == day
                            );

                            new Updater().Upload(GetSolvers(tsolver)[0]).Wait();
                        };
                    }
                ) ??
                Command(
                    args,
                    Args("upload", "today"),
                    m =>
                    {
                        DateTime dt = DateTime.UtcNow.AddHours(-5);
                        if (dt is { Month: 12, Day: >= 1 and <= 25, })
                        {
                            Type tsolver = tsolvers.First(
                                tsolver => SolverExtensions.Year(tsolver) == dt.Year &&
                                           SolverExtensions.Day(tsolver) == dt.Day
                            );

                            return () => new Updater().Upload(GetSolvers(tsolver)[0]).Wait();
                        }

                        throw new AocCommuncationException("Event is not active. This option works in Dec 1-25 only)");
                    }
                ) ??
                Command(
                    args,
                    Args("([0-9]+)/(Day)?([0-9]+)"),
                    m =>
                    {
                        int year = int.Parse(m[0]);
                        int day = int.Parse(m[2]);
                        Type tsolversSelected = tsolvers.First(
                            tsolver => SolverExtensions.Year(tsolver) == year && SolverExtensions.Day(tsolver) == day
                        );
                        return () => Runner.RunAll(GetSolvers(tsolversSelected));
                    }
                ) ??
                Command(
                    args,
                    Args("[0-9]+"),
                    m =>
                    {
                        int year = int.Parse(m[0]);
                        IEnumerable<Type> tsolversSelected
                            = tsolvers.Where(tsolver => SolverExtensions.Year(tsolver) == year);
                        return () => Runner.RunAll(GetSolvers(tsolversSelected.ToArray()));
                    }
                ) ??
                Command(
                    args,
                    Args("([0-9]+)/all"),
                    m =>
                    {
                        int year = int.Parse(m[0]);
                        IEnumerable<Type> tsolversSelected
                            = tsolvers.Where(tsolver => SolverExtensions.Year(tsolver) == year);
                        return () => Runner.RunAll(GetSolvers(tsolversSelected.ToArray()));
                    }
                ) ??
                Command(args, Args("all"), m => { return () => Runner.RunAll(GetSolvers(tsolvers)); }) ??
                Command(
                    args,
                    Args("today"),
                    m =>
                    {
                        DateTime dt = DateTime.UtcNow.AddHours(-5);
                        if (dt is { Month: 12, Day: >= 1 and <= 25, })
                        {
                            Type tsolversSelected = tsolvers.First(
                                tsolver => SolverExtensions.Year(tsolver) == dt.Year &&
                                           SolverExtensions.Day(tsolver) == dt.Day
                            );

                            return () => Runner.RunAll(GetSolvers(tsolversSelected));
                        }

                        throw new AocCommuncationException("Event is not active. This option works in Dec 1-25 only)");
                    }
                ) ??
                Command(
                    args,
                    Args("calendars"),
                    _ =>
                    {
                        return () =>
                        {
                            Type[] tsolversSelected = (from tsolver in tsolvers
                                group tsolver by SolverExtensions.Year(tsolver)
                                into g
                                orderby SolverExtensions.Year(g.First()) descending
                                select g.First()).ToArray();

                            ISolver[] solvers = GetSolvers(tsolversSelected);
                            foreach (ISolver solver in solvers)
                            {
                                solver.SplashScreen().Show();
                            }
                        };
                    }
                ) ??
                (() => Console.WriteLine(usage));

try
{
    action();
}
catch (AggregateException a)
{
    if (a.InnerExceptions.Count == 1 && a.InnerException is AocCommuncationException)
    {
        Console.WriteLine(a.InnerException.Message);
    }
    else
    {
        throw;
    }
}

ISolver[] GetSolvers(params Type[] tsolver)
{
    return tsolver.Select(t => Activator.CreateInstance(t) as ISolver).ToArray();
}

Action Command(string[] args, string[] regexes, Func<string[], Action> parse)
{
    if (args.Length != regexes.Length)
    {
        return null;
    }

    IEnumerable<Match> matches = args.Zip(regexes, (arg, regex) => new Regex("^" + regex + "$").Match(arg));
    if (!matches.All(match => match.Success))
    {
        return null;
    }

    try
    {
        return parse(
            matches.SelectMany(
                    m => m.Groups.Count > 1 ? m.Groups.Cast<Group>().Skip(1).Select(g => g.Value) : new[] { m.Value, }
                ).
                ToArray()
        );
    }
    catch
    {
        return null;
    }
}

string[] Args(params string[] regex)
{
    return regex;
}
