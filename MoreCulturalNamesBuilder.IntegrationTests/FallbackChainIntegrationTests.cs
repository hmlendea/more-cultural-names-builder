using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class FallbackChainIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(FallbackChainIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenFallbackLocations_WhenBuildingTheCk2Mod_ThenEachTitleResolvesThroughTheChain()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildFallbackRequest("CK2"));

            string landedTitles = TextContent.ReadWindows1252(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(landedTitles, Does.Contain("c_alpha = {"));
                Assert.That(landedTitles, Does.Contain("english = \"Alpha\" # Language=English"));
                Assert.That(landedTitles, Does.Contain("c_beta = {"));
                Assert.That(landedTitles, Does.Contain("french = \"Alpha\" # Language=English"));
                Assert.That(landedTitles, Does.Contain("c_gamma = {"));
                Assert.That(landedTitles, Does.Contain("c_epsilon = {"));
                Assert.That(landedTitles, Does.Contain("french = \"Epsilon\" # Language=Romanian"));
            });
        }

        [Test]
        public void GivenFallbackLocations_WhenBuildingTheCk3Mod_ThenEachTitleResolvesThroughTheChain()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildFallbackRequest("CK3"));

            string landedTitles = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(landedTitles, Does.Contain("name_list_romanian = cn_Alpha_romanian # Alpha # Language=English"));
                Assert.That(landedTitles, Does.Contain("c_epsilon = {"));
                Assert.That(landedTitles, Does.Contain("name_list_romanian = cn_Epsilon_romanian # Epsilon # Language=Romanian"));
            });
        }

        [Test]
        public void GivenFallbackLocations_WhenBuildingTheHoI4Mod_ThenTheChainResolvesInLocalisation()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildFallbackRequest("HOI4"));

            string localisation = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "HOI4",
                "more-cultural-names",
                "localisation",
                "english",
                "zzz999_more-cultural-names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("english_STATE_1:0 \"Alpha\""));
                Assert.That(localisation, Does.Contain("english_STATE_2:0 \"Alpha\""));
                Assert.That(localisation, Does.Contain("french_STATE_1:0 \"Alpha\""));
                Assert.That(localisation, Does.Contain("french_STATE_2:0 \"Alpha\""));
            });
        }

        [Test]
        public void GivenFallbackLocations_WhenBuildingTheImperatorRomeMod_ThenTheChainResolvesInProvinceNames()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildFallbackRequest("IR"));

            string provinceNames = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "IR",
                "more-cultural-names",
                "common",
                "province_names",
                "romanian.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(provinceNames, Does.Contain("1 = PROV1_romanian # Alpha # Language=English"));
                Assert.That(provinceNames, Does.Contain("2 = PROV2_romanian # Alpha # Language=English"));
            });
        }

        [Test]
        public void GivenFallbackLanguages_WhenBuildingTheCk2Mod_ThenTheFallbackChainIsApplied()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildFallbackRequest("CK2"));

            string localisation = TextContent.ReadWindows1252(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "localisation",
                "000_more-cultural-names_landed_titles.csv"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("c_alpha;Alpha;Alpha;Alpha;;Alpha;;;;;;;;;x"));
                Assert.That(localisation, Does.Contain("c_alpha_adj;Alphan;"));
            });
        }

        private static ModBuildRequest BuildFallbackRequest(string game)
        {
            ModBuildRequest request = new()
            {
                Game = game,
                LanguageStorePath = TestPaths.FallbackLanguagesFilePath,
                LocationStorePath = TestPaths.FallbackLocationsFilePath
            };

            if (string.Equals(game, "CK2", StringComparison.Ordinal))
            {
                request.LandedTitlesFilePath = TestPaths.FallbackCk2LandedTitlesFilePath;
                request.LandedTitlesFileName = "00_landed_titles.txt";
            }

            if (string.Equals(game, "CK3", StringComparison.Ordinal))
            {
                request.LandedTitlesFilePath = TestPaths.FallbackCk3LandedTitlesFilePath;
                request.LandedTitlesFileName = "00_landed_titles.txt";
            }

            return request;
        }
    }
}
