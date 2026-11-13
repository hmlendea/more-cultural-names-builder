using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    internal static partial class CK3CulturalNameBlockMerger
    {
        [GeneratedRegex("^\\s*[ekdcb]_.*=\\s*\\{.*$")]
        private static partial Regex TitleRegex { get; }

        [GeneratedRegex("^\\s*cultural_names\\s*=\\s*\\{.*$")]
        private static partial Regex CulturalNamesRegex { get; }

        [GeneratedRegex("^\\s*(name_list_\\S+)\\s*=.*$")]
        private static partial Regex NameListRegex { get; }

        internal static string Merge(string content)
        {
            List<string> lines = [.. content.Split('\n')];
            Dictionary<int, List<string>> replacementBlocks = [];
            HashSet<int> omittedLines = [];

            for (int lineIndex = 0; lineIndex < lines.Count; lineIndex += 1)
            {
                if (!TitleRegex.IsMatch(lines[lineIndex]))
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

            if (replacementBlocks.Count == 0)
            {
                return content;
            }

            List<string> mergedContentLines = [];

            for (int lineIndex = 0; lineIndex < lines.Count; lineIndex += 1)
            {
                if (omittedLines.Contains(lineIndex))
                {
                    continue;
                }

                if (replacementBlocks.TryGetValue(lineIndex, out List<string>? replacementBlock))
                {
                    mergedContentLines.Add(string.Join(Environment.NewLine, replacementBlock));
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
                string line = lines[lineIndex];

                for (int characterIndex = 0; characterIndex < line.Length; characterIndex += 1)
                {
                    if (line[characterIndex] == '{')
                    {
                        blockDepth += 1;
                    }
                    else if (line[characterIndex] == '}')
                    {
                        blockDepth -= 1;
                    }
                }

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
                if (!CulturalNamesRegex.IsMatch(lines[lineIndex]))
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
                    Match nameListMatch = NameListRegex.Match(lines[lineIndex]);

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
                int firstLineToOmit = culturalNameBlock.StartLineIndex + 1;

                if (culturalNameBlock.StartLineIndex != firstCulturalNameBlock.StartLineIndex)
                {
                    firstLineToOmit = culturalNameBlock.StartLineIndex;
                }

                for (int lineIndex = firstLineToOmit;
                    lineIndex <= culturalNameBlock.EndLineIndex;
                    lineIndex += 1)
                {
                    omittedLines.Add(lineIndex);
                }
            }
        }
    }
}
