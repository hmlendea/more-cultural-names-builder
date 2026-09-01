using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    internal static class CK3CulturalNameBlockMerger
    {
        internal static string RemoveDuplicateScoreDefinitions(string content)
        {
            HashSet<string> scoreDefinitions = [];
            List<string> lines = [];
            List<string> sourceLines = content.Split('\n').ToList();

            for (int lineIndex = 0; lineIndex < sourceLines.Count; lineIndex += 1)
            {
                string line = sourceLines[lineIndex];

                if (Regex.IsMatch(line, "^\\s*@.*=.*$", RegexOptions.Compiled) &&
                    !scoreDefinitions.Add(line.Trim()))
                {
                    lineIndex = FindBlockEndLineIndex(sourceLines, lineIndex);
                    continue;
                }

                lines.Add(line);
            }

            return string.Join(Environment.NewLine, lines);
        }

        internal static string Merge(string content)
        {
            List<string> lines = content.Split('\n').ToList();
            Dictionary<int, List<string>> replacementBlocks = [];
            HashSet<int> omittedLines = [];

            for (int lineIndex = 0; lineIndex < lines.Count; lineIndex += 1)
            {
                if (!Regex.IsMatch(lines[lineIndex], "^\\s*[ekdcb]_.*=\\s*\\{.*$", RegexOptions.Compiled))
                {
                    continue;
                }

                int titleEndLineIndex = FindBlockEndLineIndex(lines, lineIndex);
                List<CulturalNameBlock> culturalNameBlocks = FindCulturalNameBlocks(
                    lines,
                    lineIndex + 1,
                    titleEndLineIndex);

                if (culturalNameBlocks.Count > 1)
                {
                    AddMergedCulturalNameBlock(culturalNameBlocks, lines, replacementBlocks, omittedLines);
                }

                lineIndex = titleEndLineIndex;
            }

            List<string> mergedContentLines = [];

            for (int lineIndex = 0; lineIndex < lines.Count; lineIndex += 1)
            {
                if (omittedLines.Contains(lineIndex))
                {
                    continue;
                }

                if (replacementBlocks.ContainsKey(lineIndex))
                {
                    mergedContentLines.Add(string.Join(Environment.NewLine, replacementBlocks[lineIndex]));
                    continue;
                }

                mergedContentLines.Add(lines[lineIndex]);
            }

            return string.Join(Environment.NewLine, mergedContentLines);
        }

        private static int FindBlockEndLineIndex(IList<string> lines, int blockStartLineIndex)
        {
            int blockDepth = 0;

            for (int lineIndex = blockStartLineIndex; lineIndex < lines.Count; lineIndex += 1)
            {
                blockDepth += lines[lineIndex].Count(character => character.Equals('{'));
                blockDepth -= lines[lineIndex].Count(character => character.Equals('}'));

                if (blockDepth == 0)
                {
                    return lineIndex;
                }
            }

            return lines.Count - 1;
        }

        private static List<CulturalNameBlock> FindCulturalNameBlocks(
            IList<string> lines,
            int firstLineIndex,
            int lastLineIndex)
        {
            List<CulturalNameBlock> culturalNameBlocks = [];

            for (int lineIndex = firstLineIndex; lineIndex < lastLineIndex; lineIndex += 1)
            {
                if (!Regex.IsMatch(lines[lineIndex], "^\\s*cultural_names\\s*=\\s*\\{.*$", RegexOptions.Compiled))
                {
                    continue;
                }

                culturalNameBlocks.Add(new CulturalNameBlock(
                    lineIndex,
                    FindBlockEndLineIndex(lines, lineIndex)));
            }

            return culturalNameBlocks;
        }

        private static void AddMergedCulturalNameBlock(
            IEnumerable<CulturalNameBlock> culturalNameBlocks,
            IList<string> lines,
            IDictionary<int, List<string>> replacementBlocks,
            ISet<int> omittedLines)
        {
            CulturalNameBlock firstCulturalNameBlock = culturalNameBlocks.First();
            HashSet<string> nameListKeys = [];
            List<string> mergedLines = [lines[firstCulturalNameBlock.StartLineIndex]];

            foreach (CulturalNameBlock culturalNameBlock in culturalNameBlocks)
            {
                for (int lineIndex = culturalNameBlock.StartLineIndex + 1;
                    lineIndex < culturalNameBlock.EndLineIndex;
                    lineIndex += 1)
                {
                    Match nameListMatch = Regex.Match(
                        lines[lineIndex],
                        "^\\s*(name_list_\\S+)\\s*=.*$",
                        RegexOptions.Compiled);

                    if (!nameListMatch.Success || nameListKeys.Add(nameListMatch.Groups[1].Value))
                    {
                        mergedLines.Add(lines[lineIndex]);
                    }
                }
            }

            mergedLines.Add(lines[firstCulturalNameBlock.EndLineIndex]);
            replacementBlocks.Add(firstCulturalNameBlock.StartLineIndex, mergedLines);

            foreach (CulturalNameBlock culturalNameBlock in culturalNameBlocks)
            {
                for (int lineIndex = culturalNameBlock.StartLineIndex + 1;
                    lineIndex <= culturalNameBlock.EndLineIndex;
                    lineIndex += 1)
                {
                    omittedLines.Add(lineIndex);
                }
            }
        }
    }
}