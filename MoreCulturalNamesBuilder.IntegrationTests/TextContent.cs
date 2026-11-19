using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    internal static class TextContent
    {
        private static string Normalise(string content) =>
            content.Replace("\r\n", "\n", StringComparison.Ordinal);

        internal static string Read(string filePath) =>
            Normalise(File.ReadAllText(filePath, Encoding.UTF8));

        internal static string ReadWindows1252(string filePath) =>
            Normalise(File.ReadAllText(filePath, TestPaths.Windows1252Encoding));

        internal static string ReadExpected(string relativeFilePath) =>
            Read(Path.Combine(TestPaths.ExpectedDirectory, relativeFilePath));

        internal static string ReadExpectedWindows1252(string relativeFilePath) =>
            ReadWindows1252(Path.Combine(TestPaths.ExpectedDirectory, relativeFilePath));

        internal static byte[] ReadBytes(string filePath) => File.ReadAllBytes(filePath);

        internal static string WithoutVersionComment(string content)
        {
            List<string> retainedLines = [];
            int lineIndex = 0;

            foreach (string line in Normalise(content).Split('\n'))
            {
                bool isVersionComment = lineIndex is 0 && line.StartsWith("# Version ", StringComparison.Ordinal);

                if (!isVersionComment)
                {
                    retainedLines.Add(line);
                }

                lineIndex += 1;
            }

            return string.Join('\n', retainedLines);
        }
    }
}
