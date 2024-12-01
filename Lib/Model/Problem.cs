namespace AdventOfCode.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Dom;

internal class Problem
{
    public string[] Answers { get; private set; }
    public string ContentMd { get; private set; }
    public int Day { get; private set; }
    public string Input { get; private set; }
    public string Title { get; private set; }
    public int Year { get; private set; }

    public static Problem Parse(int year, int day, string url, IDocument document, string input)
    {
        string md = ParseMdIntro(document, 3, url);

        List<string> answers = ParseAnswers(document);
        string title = document.QuerySelector("h2").TextContent;

        Match match = Regex.Match(title, ".*: (.*) ---");
        if (match.Success)
        {
            title = match.Groups[1].Value;
        }

        return new Problem
        {
            Year = year,
            Day = day,
            Title = title,
            ContentMd = md,
            Input = input,
            Answers = answers.ToArray(),
        };
    }

    private static List<string> ParseAnswers(IDocument document)
    {
        var answers = new List<string>();
        foreach (INode startNode in document.QuerySelectorAll("article").Select(article => article.NextSibling))
        {
            INode answerNode = startNode;
            while (answerNode != null &&
                   !(answerNode.NodeName == "P" &&
                     (answerNode as IElement)?.QuerySelector("code") != null &&
                     answerNode.TextContent.Contains("answer")))
            {
                answerNode = answerNode.NextSibling as IElement;
            }

            IElement code = (answerNode as IElement)?.QuerySelector("code");
            if (code != null)
            {
                answers.Add(code.TextContent);
            }
        }

        return answers;
    }

    private static string ParseMd(IDocument document)
    {
        return document.QuerySelectorAll("article").
            Aggregate("", (current, article) => current + UnparseList("", article) + "\n");
    }

    private static string ParseMdIntro(IDocument document, int paragraphs, string url)
    {
        List<string> article = ParseMd(document).Split("\n\n").ToList();
        article = article.Take(Math.Min(paragraphs, article.Count)).ToList();
        article.Add($"Read the [full puzzle]({url}).\n");
        return string.Join("\n\n", article);
    }

    private static IEnumerable<string> Unparse(INode node)
    {
        switch (node.NodeName.ToLower())
        {
        case "h2":
            yield return "## " + UnparseList("", node) + "\n";
            break;
        case "p":
            yield return UnparseList("", node) + "\n";
            break;
        case "em":
            yield return "<em>" + UnparseList("", node) + "</em>";
            break;
        case "code":
            yield return "<code>" + UnparseList("", node) + "</code>";
            break;
        case "span":
            yield return UnparseList("", node);
            break;
        case "s":
            yield return "~~" + UnparseList("", node) + "~~";
            break;
        case "ul":
            foreach (string unparsed in node.ChildNodes.SelectMany(Unparse))
            {
                yield return unparsed;
            }

            break;
        case "li":
            yield return " - " + UnparseList("", node);
            break;
        case "pre":
            yield return "<pre>\n";
            bool freshLine = true;
            foreach (INode item in node.ChildNodes)
            {
                foreach (string unparsed in Unparse(item))
                {
                    freshLine = unparsed[unparsed.Length - 1] == '\n';
                    yield return unparsed;
                }
            }

            if (freshLine)
            {
                yield return "</pre>\n";
            }
            else
            {
                yield return "\n</pre>\n";
            }

            break;
        case "a":
            yield return "[" + UnparseList("", node) + "](" + (node as IElement).Attributes["href"].Value + ")";
            break;
        case "br":
            yield return "\n";
            break;
        case "#text":
            yield return node.TextContent;
            break;
        default:
            throw new AggregateException(node.NodeName);
        }
    }

    private static string UnparseList(string sep, INode element)
    {
        return string.Join(sep, element.ChildNodes.SelectMany(Unparse));
    }
}
