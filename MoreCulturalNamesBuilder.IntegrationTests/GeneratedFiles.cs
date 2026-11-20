using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    internal static class GeneratedFiles
    {
        internal static IEnumerable<string> FindDifferences(string firstDirectoryPath, string secondDirectoryPath)
        {
            IEnumerable<string> relativeFilePaths = Directory
                .GetFiles(firstDirectoryPath, "*", SearchOption.AllDirectories)
                .Select(filePath => Path.GetRelativePath(firstDirectoryPath, filePath))
                .Order(StringComparer.Ordinal);
            List<string> differences = [];

            foreach (string relativeFilePath in relativeFilePaths)
            {
                string firstFilePath = Path.Combine(firstDirectoryPath, relativeFilePath);
                string secondFilePath = Path.Combine(secondDirectoryPath, relativeFilePath);

                if (!File.Exists(secondFilePath) || !AreEquivalent(firstFilePath, secondFilePath))
                {
                    differences.Add(relativeFilePath);
                }
            }

            return differences;
        }

        private static bool AreEquivalent(string firstFilePath, string secondFilePath)
        {
            if (TextContent.ReadBytes(firstFilePath).SequenceEqual(TextContent.ReadBytes(secondFilePath)))
            {
                return true;
            }

            return string.Equals(
                TextContent.WithoutVersionComment(TextContent.Read(firstFilePath)),
                TextContent.WithoutVersionComment(TextContent.Read(secondFilePath)),
                StringComparison.Ordinal);
        }
    }
}
