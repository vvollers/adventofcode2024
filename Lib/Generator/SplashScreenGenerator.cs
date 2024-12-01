namespace AdventOfCode.Generator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;

internal class SplashScreenGenerator
{
    public string Generate(Calendar calendar)
    {
        string calendarPrinter = this.CalendarPrinter(calendar);
        return $$"""
                 using System;

                 namespace AdventOfCode.Y{{calendar.Year}};

                 class SplashScreenImpl : ISplashScreen {
                 
                     public void Show() {
                 
                         var color = Console.ForegroundColor;
                         {{calendarPrinter.Indent(12)}}
                         Console.ForegroundColor = color;
                         Console.WriteLine();
                     }
                 
                     private static void Write(int rgb, bool bold, string text){
                        Console.Write($"\u001b[38;2;{(rgb>>16)&255};{(rgb>>8)&255};{rgb&255}{(bold ? ";1" : "")}m{text}");
                     }
                 }
                 """;
    }

    private string CalendarPrinter(Calendar calendar)
    {
        List<IEnumerable<CalendarToken>> lines = calendar.Lines.
            Select(line => new[] { new CalendarToken { Text = "           ", }, }.Concat(line)).
            ToList();

        var bw = new BufferWriter();
        foreach (IEnumerable<CalendarToken> line in lines)
        {
            foreach (CalendarToken token in line)
            {
                bw.Write(token.ConsoleColor, token.Text, token.Bold);
            }

            bw.Write(-1, "\n", false);
        }

        return bw.GetContent();
    }

    private sealed class BufferWriter
    {
        private readonly StringBuilder sb = new();
        private string buffer = "";
        private bool bufferBold;
        private int bufferColor = -1;

        public string GetContent()
        {
            this.Flush();
            return this.sb.ToString();
        }

        public void Write(int color, string text, bool bold)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (!string.IsNullOrWhiteSpace(this.buffer) && (color != this.bufferColor || this.bufferBold != bold))
                {
                    this.Flush();
                }

                this.bufferColor = color;
                this.bufferBold = bold;
            }

            this.buffer += text;
        }

        private void Flush()
        {
            while (this.buffer.Length > 0)
            {
                string block = this.buffer.Substring(0, Math.Min(100, this.buffer.Length));
                this.buffer = this.buffer.Substring(block.Length);
                block = block.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
                this.sb.AppendLine(
                    $@"Write(0x{this.bufferColor.ToString("x")}, {this.bufferBold.ToString().ToLower()}, ""{block}"");"
                );
            }

            this.buffer = "";
        }
    }
}
