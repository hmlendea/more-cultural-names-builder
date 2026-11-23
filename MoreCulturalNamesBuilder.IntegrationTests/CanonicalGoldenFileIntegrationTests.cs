using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class CanonicalGoldenFileIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(CanonicalGoldenFileIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenTheWholeTreeMatchesTheGoldenFiles(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            string generatedDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, game);
            string goldenDirectoryPath = Path.Combine(TestPaths.ExpectedDirectory, "canonical", game);

            Assert.Multiple(() =>
            {
                Assert.That(
                    GeneratedFiles.FindDifferences(goldenDirectoryPath, generatedDirectoryPath),
                    Is.Empty,
                    "Every golden file must be reproduced.");
                Assert.That(
                    GeneratedFiles.FindDifferences(generatedDirectoryPath, goldenDirectoryPath),
                    Is.Empty,
                    "No file may be generated that is not in the golden tree.");
            });
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenTheGeneratedInventoryMatchesTheGoldenInventory(
            string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            string generatedDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, game);
            string goldenDirectoryPath = Path.Combine(TestPaths.ExpectedDirectory, "canonical", game);

            Assert.That(RelativePaths(generatedDirectoryPath), Is.EqualTo(RelativePaths(goldenDirectoryPath)));
        }

        private static ModBuildRequest BuildRequest(string game)
            => new()
            {
                Game = game,
                LandedTitlesFilePath = ResolveLandedTitlesFilePath(game),
                LandedTitlesFileName = ResolveLandedTitlesFileName(game)
            };

        private static string? ResolveLandedTitlesFilePath(string game)
        {
            if (string.Equals(game, "CK2", StringComparison.Ordinal))
            {
                return TestPaths.Ck2LandedTitlesFilePath;
            }

            if (string.Equals(game, "CK3", StringComparison.Ordinal))
            {
                return TestPaths.Ck3LandedTitlesFilePath;
            }

            return null;
        }

        private static string? ResolveLandedTitlesFileName(string game)
        {
            if (string.Equals(game, "CK2", StringComparison.Ordinal) ||
                string.Equals(game, "CK3", StringComparison.Ordinal))
            {
                return "00_landed_titles.txt";
            }

            return null;
        }

        private static IEnumerable<string> RelativePaths(string directoryPath)
            => Directory
                .GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                .Select(filePath => Path.GetRelativePath(directoryPath, filePath))
                .Order(StringComparer.Ordinal);
    }
}
