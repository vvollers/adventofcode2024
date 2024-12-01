namespace AdventOfCode;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using Generator;
using Model;

internal class Updater
{
#pragma warning disable S1075
    private const string BaseUrl = "https://adventofcode.com/";
#pragma warning restore S1075

    public static Uri BaseAddress
    {
        get => new(BaseUrl);
    }

    public static async Task Update(int year, int day)
    {
        string session = GetSession();
        var baseAddress = new Uri(BaseUrl);

        var requester = new DefaultHttpRequester("github.com/encse/adventofcode by encse@csokavar.hu");

        IBrowsingContext context = BrowsingContext.New(
            Configuration.Default.With(requester).WithDefaultLoader().WithCss().WithDefaultCookies()
        );
        context.SetCookie(new Url(baseAddress.ToString()), "session=" + session);

        Calendar calendar = await DownloadCalendar(context, baseAddress, year);
        Problem problem = await DownloadProblem(context, baseAddress, year, day);

        string dir = Dir(year, day);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        UpdateReadmeForYear(calendar);
        UpdateSplashScreen(calendar);
        UpdateReadmeForDay(problem);
        UpdateInput(problem);
        UpdateRefout(problem);
        UpdateSolutionTemplate(problem);
    }

    public async Task Upload(ISolver solver)
    {
        ConsoleColor color = Console.ForegroundColor;
        Console.WriteLine();
        SolverResult solverResult = Runner.RunSolver(solver);
        Console.WriteLine();

        if (solverResult.errors.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Uhh-ohh the solution doesn't pass the tests...");
            Console.ForegroundColor = color;
            Console.WriteLine();
            return;
        }

        Problem problem = await DownloadProblem(GetContext(), BaseAddress, solver.Year(), solver.Day());

        if (problem.Answers.Length == 2)
        {
            Console.WriteLine("Both parts of this puzzle are complete!");
            Console.WriteLine();
        }
        else if (solverResult.answers.Length <= problem.Answers.Length)
        {
            Console.WriteLine($"You need to work on part {problem.Answers.Length + 1}");
            Console.WriteLine();
        }
        else
        {
            int level = problem.Answers.Length + 1;
            string answer = solverResult.answers[problem.Answers.Length];
            Console.WriteLine($"Uploading answer ({answer}) for part {level}...");

            // https://adventofcode.com/{year}/day/{day}/answer
            // level={part}&answer={answer}

            var cookieContainer = new CookieContainer();
            using var handler = new HttpClientHandler { CookieContainer = cookieContainer, };
            using var client = new HttpClient(handler) { BaseAddress = BaseAddress, };

            var content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("level", level.ToString()),
                    new KeyValuePair<string, string>("answer", answer),
                }
            );

            cookieContainer.Add(BaseAddress, new Cookie("session", GetSession()));
            HttpResponseMessage result = await client.PostAsync($"/{solver.Year()}/day/{solver.Day()}/answer", content);
            result.EnsureSuccessStatusCode();
            string responseString = await result.Content.ReadAsStringAsync();

            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument document = await context.OpenAsync(req => req.Content(responseString));
            string article = document.Body.QuerySelector("body > main > article").TextContent;
            article = Regex.Replace(article, @"\[Continue to Part Two.*", "", RegexOptions.Singleline);
            article = Regex.Replace(article, @"You have completed Day.*", "", RegexOptions.Singleline);
            article = Regex.Replace(article, @"\(You guessed.*", "", RegexOptions.Singleline);
            article = Regex.Replace(article, @"  ", "\n", RegexOptions.Singleline);

            if (article.StartsWith("That's the right answer") || article.Contains("You've finished every puzzle"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(article);
                Console.ForegroundColor = color;
                Console.WriteLine();
                await Update(solver.Year(), solver.Day());
            }
            else if (article.StartsWith("That's not the right answer") ||
                     article.StartsWith("You gave an answer too recently"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(article);
                Console.ForegroundColor = color;
                Console.WriteLine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(article);
                Console.ForegroundColor = color;
            }
        }
    }

    private static string Dir(int year, int day)
    {
        return SolverExtensions.WorkingDir(year, day);
    }

    private static async Task<Calendar> DownloadCalendar(IBrowsingContext context, Uri baseUri, int year)
    {
        IDocument document = await context.OpenAsync(baseUri.ToString() + year);
        if (document.StatusCode != HttpStatusCode.OK)
        {
            throw new AocCommuncationException("Could not fetch calendar", document.StatusCode, document.TextContent);
        }

        return Calendar.Parse(year, document);
    }

    private static async Task<Problem> DownloadProblem(IBrowsingContext context, Uri baseUri, int year, int day)
    {
        string uri = baseUri + $"{year}/day/{day}";
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Updating " + uri);
        Console.ForegroundColor = color;

        IDocument problemStatement = await context.OpenAsync(uri);
        IResponse input = await context.GetService<IDocumentLoader>().
            FetchAsync(new DocumentRequest(new Url(baseUri + $"{year}/day/{day}/input"))).
            Task;

        if (input.StatusCode != HttpStatusCode.OK)
        {
            throw new AocCommuncationException(
                "Could not fetch input",
                input.StatusCode,
                await new StreamReader(input.Content).ReadToEndAsync()
            );
        }

        return Problem.Parse(
            year,
            day,
            baseUri + $"{year}/day/{day}",
            problemStatement,
            await new StreamReader(input.Content).ReadToEndAsync()
        );
    }

    private static IBrowsingContext GetContext()
    {
        IBrowsingContext context
            = BrowsingContext.New(Configuration.Default.WithDefaultLoader().WithCss().WithDefaultCookies());
        context.SetCookie(new Url(BaseAddress.ToString()), "session=" + GetSession());
        return context;
    }

    private static string GetSession()
    {
        if (!Environment.GetEnvironmentVariables().Contains("SESSION"))
        {
            throw new AocCommuncationException("Specify SESSION environment variable");
        }

        return Environment.GetEnvironmentVariable("SESSION");
    }

    private static void UpdateInput(Problem problem)
    {
        string file = Path.Combine(Dir(problem.Year, problem.Day), "input.in");
        WriteFile(file, problem.Input);
    }

    private static void UpdateReadmeForDay(Problem problem)
    {
        string file = Path.Combine(Dir(problem.Year, problem.Day), "README.md");
        WriteFile(file, problem.ContentMd);
    }

    private static void UpdateReadmeForYear(Calendar calendar)
    {
        string file = Path.Combine(SolverExtensions.WorkingDir(calendar.Year), "README.md");
        WriteFile(file, ReadmeGeneratorForYear.Generate(calendar));

        string svg = Path.Combine(SolverExtensions.WorkingDir(calendar.Year), "calendar.svg");
        WriteFile(svg, calendar.ToSvg());
    }

    private static void UpdateRefout(Problem problem)
    {
        string file = Path.Combine(Dir(problem.Year, problem.Day), "input.refout");
        if (problem.Answers.Any())
        {
            WriteFile(file, string.Join("\n", problem.Answers));
        }
    }

    private static void UpdateSolutionTemplate(Problem problem)
    {
        string file = Path.Combine(Dir(problem.Year, problem.Day), "Solution.cs");
        if (!File.Exists(file))
        {
            WriteFile(file, SolutionTemplateGenerator.Generate(problem));
        }
    }

    private static void UpdateSplashScreen(Calendar calendar)
    {
        string file = Path.Combine(SolverExtensions.WorkingDir(calendar.Year), "SplashScreen.cs");
        WriteFile(file, new SplashScreenGenerator().Generate(calendar));
    }

    private static void WriteFile(string file, string content)
    {
        Console.WriteLine($"Writing {file}");
        File.WriteAllText(file, content);
    }
}
