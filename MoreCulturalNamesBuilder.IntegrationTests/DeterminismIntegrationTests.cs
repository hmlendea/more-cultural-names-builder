using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class DeterminismIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(DeterminismIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingFiveTimes_ThenEveryTreeIsIdentical(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            List<string> directoryPaths = [];

            for (int attempt = 0; attempt < 5; attempt += 1)
            {
                string directoryPath = Path.Combine(temporaryDirectory.DirectoryPath, $"attempt-{attempt}");
                directoryPaths.Add(directoryPath);
                ModBuildRunner.Run(directoryPath, request);
            }

            string referenceDirectoryPath = Path.Combine(directoryPaths[0], game, "more-cultural-names");

            Assert.Multiple(() =>
            {
                foreach (string directoryPath in directoryPaths.Skip(1))
                {
                    Assert.That(
                        GeneratedFiles.FindDifferences(
                            referenceDirectoryPath,
                            Path.Combine(directoryPath, game, "more-cultural-names")),
                        Is.Empty,
                        directoryPath);
                }
            });
        }

        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingTheMod_ThenTheLocalisationFileOrderIsStable(string game)
        {
            ModBuildRequest request = BuildRequest(game);

            ModBuildRunner.Run(Path.Combine(temporaryDirectory.DirectoryPath, "first"), request);
            ModBuildRunner.Run(Path.Combine(temporaryDirectory.DirectoryPath, "second"), request);

            List<List<string>> fileOrders =
            [
                .. new[] { "first", "second" }.Select(directoryName => Directory
                    .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, directoryName, game), "*.yml", SearchOption.AllDirectories)
                    .Select(filePath => Path.GetRelativePath(Path.Combine(temporaryDirectory.DirectoryPath, directoryName), filePath))
                    .Order(StringComparer.Ordinal)
                    .ToList())
            ];

            Assert.That(fileOrders[1], Is.EqualTo(fileOrders[0]));
        }

        [TestCase("CK3", "more-cultural-names_titles_l_english.yml")]
        [TestCase("EU5", "more_cultural_names_location_names_l_english.yml")]
        [TestCase("IR", "more-cultural-names_provincenames_l_english.yml")]
        [TestCase("Vic3", "more_cultural_names_state_names_l_english.yml")]
        public void GivenTheCanonicalDataStores_WhenBuildingTheMod_ThenTheEnglishBodyOrderIsStable(string game, string fileName)
        {
            ModBuildRequest request = BuildRequest(game);

            ModBuildRunner.Run(Path.Combine(temporaryDirectory.DirectoryPath, "first"), request);
            ModBuildRunner.Run(Path.Combine(temporaryDirectory.DirectoryPath, "second"), request);

            string firstEnglishPath = Directory
                .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, "first", game), fileName, SearchOption.AllDirectories)
                .Single();
            string secondEnglishPath = Directory
                .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, "second", game), fileName, SearchOption.AllDirectories)
                .Single();

            Assert.That(TextContent.Read(secondEnglishPath), Is.EqualTo(TextContent.Read(firstEnglishPath)));
        }

        private static ModBuildRequest BuildRequest(string game)
        {
            ModBuildRequest request = new() { Game = game };

            if (string.Equals(game, "CK2", StringComparison.Ordinal))
            {
                request.LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath;
                request.LandedTitlesFileName = "00_landed_titles.txt";
            }

            if (string.Equals(game, "CK3", StringComparison.Ordinal))
            {
                request.LandedTitlesFilePath = TestPaths.Ck3LandedTitlesFilePath;
                request.LandedTitlesFileName = "00_landed_titles.txt";
            }

            return request;
        }
    }
}
