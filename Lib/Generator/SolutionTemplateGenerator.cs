using AdventOfCode.Model;

namespace AdventOfCode.Generator;

static class SolutionTemplateGenerator {
    public static string Generate(Problem problem) {
        return $$"""
            namespace AdventOfCode.Y{{problem.Year}}.Day{{problem.Day:00}};

            using System;
            using System.Collections.Generic;
            using System.Collections.Immutable;
            using System.Linq;
            using System.Text.RegularExpressions;
            using System.Text;
            
            [ProblemName("{{problem.Title}}")]
            class Solution : ISolver {
            
                public object PartOne(string input) {
                    return 0;
                }
            
                public object PartTwo(string input) {
                    return 0;
                }
            }
            """;
    }
}
