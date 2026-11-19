using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.ArchitectureTests
{
    [TestFixture]
    public sealed class IntegrationFixtureTests
    {
        private static readonly string fixturesDirectoryPath =
            Path.Combine(RepositorySources.Root, "MoreCulturalNamesBuilder.IntegrationTests", "TestData");

        private static readonly string testPathsFilePath =
            Path.Combine(RepositorySources.Root, "MoreCulturalNamesBuilder.IntegrationTests", "TestPaths.cs");

        private static readonly string missingFixturePrefix = "missing_";

        [Test]
        public void DeclaredFixturesMustExistAndEveryFixtureFileMustBeDeclared()
        {
            string[] declaredFileNames = GetDeclaredFileNames();
            string[] fileNames = Directory.GetFiles(fixturesDirectoryPath)
                .Select(Path.GetFileName)
                .OfType<string>()
                .Order(StringComparer.Ordinal)
                .ToArray();
            List<string> violations = [];

            Assert.That(declaredFileNames, Is.Not.Empty, "No integration fixtures were selected.");

            foreach (string declaredFileName in declaredFileNames)
            {
                if (!declaredFileName.StartsWith(missingFixturePrefix, StringComparison.Ordinal) &&
                    !fileNames.Contains(declaredFileName, StringComparer.Ordinal))
                {
                    violations.Add($"{declaredFileName}: Declared fixture is absent.");
                }
            }

            foreach (string fileName in fileNames)
            {
                if (!declaredFileNames.Contains(fileName, StringComparer.Ordinal))
                {
                    violations.Add($"{fileName}: Fixture is not declared in TestPaths.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void MissingFixtureDeclarationsMustReferToAbsentFiles()
        {
            string[] missingFileNames = GetDeclaredFileNames()
                .Where(name => name.StartsWith(missingFixturePrefix, StringComparison.Ordinal))
                .ToArray();

            Assert.That(missingFileNames, Is.Not.Empty, "No absent-fixture declarations were selected.");

            foreach (string missingFileName in missingFileNames)
            {
                Assert.That(File.Exists(Path.Combine(fixturesDirectoryPath, missingFileName)), Is.False,
                    $"{missingFileName} must remain absent so absence scenarios stay valid.");
            }
        }

        private static string[] GetDeclaredFileNames()
        {
            List<string> declaredFileNames = [];
            string declarationMarker = $"Path.Combine(TestDataDirectory, \"";

            foreach (string line in File.ReadAllLines(testPathsFilePath))
            {
                int markerIndex = line.IndexOf(declarationMarker, StringComparison.Ordinal);

                if (markerIndex < 0)
                {
                    continue;
                }

                string remainder = line[(markerIndex + declarationMarker.Length)..];
                int closingQuoteIndex = remainder.IndexOf('"');

                declaredFileNames.Add(remainder[..closingQuoteIndex]);
            }

            return [.. declaredFileNames];
        }
    }
}
