using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class GoldenTreeIntegrationTests
    {
        private static readonly string[] allGames = ["CK2", "CK3", "HOI4", "IR", "Vic3"];

        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(GoldenTreeIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenTheRichDataStores_WhenBuildingTheVerboseMod_ThenTheWholeTreeMatchesTheGoldenTree(string game)
            => AssertWholeTreeMatchesGoldenTree("rich-tree", "verbose", game, true);

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenTheRichDataStores_WhenBuildingWithoutComments_ThenTheWholeTreeMatchesTheGoldenTree(string game)
            => AssertWholeTreeMatchesGoldenTree("rich-tree", "comments-off", game, false);

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenTheFallbackDataStores_WhenBuildingTheVerboseMod_ThenTheWholeTreeMatchesTheGoldenTree(string game)
            => AssertWholeTreeMatchesGoldenTree("fallback-tree", "verbose", game, true);

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenTheFallbackDataStores_WhenBuildingWithoutComments_ThenTheWholeTreeMatchesTheGoldenTree(string game)
            => AssertWholeTreeMatchesGoldenTree("fallback-tree", "comments-off", game, false);

        [TestCase("rich-tree")]
        [TestCase("fallback-tree")]
        public void GivenAGoldenTree_WhenListingGames_ThenEveryGameIsCovered(string treeName)
        {
            string treeDirectoryPath = Path.Combine(TestPaths.ExpectedDirectory, treeName);

            Assert.Multiple(() =>
            {
                foreach (string game in allGames)
                {
                    Assert.That(
                        Directory.Exists(Path.Combine(treeDirectoryPath, "verbose", game)),
                        Is.True,
                        $"verbose/{game}");
                    Assert.That(
                        Directory.Exists(Path.Combine(treeDirectoryPath, "comments-off", game)),
                        Is.True,
                        $"comments-off/{game}");
                }
            });
        }

        private void AssertWholeTreeMatchesGoldenTree(string treeName, string mode, string game, bool areVerboseCommentsEnabled)
        {
            ModBuildRequest request = BuildRequest(treeName, game);
            request.AreVerboseCommentsEnabled = areVerboseCommentsEnabled;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string generatedDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, game);
            string goldenDirectoryPath = Path.Combine(TestPaths.ExpectedDirectory, treeName, mode, game);

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

        private static ModBuildRequest BuildRequest(string treeName, string game)
        {
            bool isRich = string.Equals(treeName, "rich-tree", StringComparison.Ordinal);
            ModBuildRequest request = new() { Game = game };

            if (isRich)
            {
                request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
                request.LocationStorePath = TestPaths.RichLocationsFilePath;
            }

            else
            {
                request.LanguageStorePath = TestPaths.FallbackLanguagesFilePath;
                request.LocationStorePath = TestPaths.FallbackLocationsFilePath;
            }

            if (string.Equals(game, "CK2", StringComparison.Ordinal))
            {
                request.LandedTitlesFilePath = ResolveCk2LandedTitlesFilePath(isRich);
                request.LandedTitlesFileName = "00_landed_titles.txt";
            }

            if (string.Equals(game, "CK3", StringComparison.Ordinal))
            {
                request.LandedTitlesFilePath = ResolveCk3LandedTitlesFilePath(isRich);
                request.LandedTitlesFileName = "00_landed_titles.txt";
            }

            return request;
        }

        private static string ResolveCk2LandedTitlesFilePath(bool isRich)
        {
            if (isRich)
            {
                return TestPaths.RichCk2LandedTitlesFilePath;
            }

            return TestPaths.FallbackCk2LandedTitlesFilePath;
        }

        private static string ResolveCk3LandedTitlesFilePath(bool isRich)
        {
            if (isRich)
            {
                return TestPaths.RichCk3LandedTitlesFilePath;
            }

            return TestPaths.FallbackCk3LandedTitlesFilePath;
        }
    }
}
