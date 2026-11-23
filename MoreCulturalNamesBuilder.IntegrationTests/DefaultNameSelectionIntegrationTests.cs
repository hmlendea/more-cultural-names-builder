using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class DefaultNameSelectionIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(DefaultNameSelectionIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenALocationWithADefaultLanguage_WhenBuildingTheCk2Mod_ThenTheDefaultNameReachesTheBareKey()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            string[] clujRows =
            [
                .. TextContent
                    .ReadWindows1252(OutputPath("CK2", "localisation", "000_more-cultural-names_landed_titles.csv"))
                    .Split('\n')
                    .Where(line => line.StartsWith("c_cluj", StringComparison.Ordinal))
            ];

            Assert.Multiple(() =>
            {
                Assert.That(clujRows, Does.Contain("c_cluj;Cluj-Napoca;Cluj-Napoca;Cluj-Napoca;;Cluj-Napoca;;;;;;;;;x"));
                Assert.That(clujRows, Does.Contain("c_cluj_adj;Clujean;Clujean;Clujean;;Clujean;;;;;;;;;x"));
                Assert.That(
                    clujRows,
                    Does.Contain("c_cluj_adj_english;Newportian;Newportian;Newportian;;Newportian;;;;;;;;;x"),
                    "Every language adjective must be emitted under the per-language key.");
            });
        }

        [Test]
        public void GivenALocationWithADefaultLanguage_WhenBuildingTheHoI4Mod_ThenTheDefaultAndAllLanguagesAreEmitted()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("HOI4"));

            string localisation = TextContent.Read(OutputPath(
                "HOI4",
                "localisation",
                "english",
                "zzz999_more-cultural-names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("romanian_STATE_42:0 \"Cluj-Napoca\""));
                Assert.That(localisation, Does.Contain("english_STATE_42:0 \"Newport\""));
            });
        }

        [Test]
        public void GivenALocationWithADefaultLanguage_WhenBuildingTheImperatorRomeMod_ThenTheBareKeyUsesTheDefault()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("IR"));

            string localisation = TextContent.Read(OutputPath(
                "IR",
                "localization",
                "more-cultural-names_provincenames_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("PROV42:0 \"Cluj-Napoca\""));
                Assert.That(localisation, Does.Contain("PROV42_english:0 \"Newport\""));
                Assert.That(localisation, Does.Contain("PROV42_romanian:0 \"Cluj-Napoca\""));
            });
        }

        [Test]
        public void GivenALocationWithADefaultLanguage_WhenBuildingTheEuropaUniversalis5Mod_ThenTheBareKeyUsesTheDefault()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("EU5"));

            string localisation = TextContent.Read(OutputPath(
                "EU5",
                "main_menu",
                "localization",
                "english",
                "replace",
                "more_cultural_names_location_names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain(" cluj: \"Cluj-Napoca\""));
                Assert.That(localisation, Does.Contain(" cluj.english_language: \"Newport\""));
                Assert.That(localisation, Does.Contain(" cluj.romanian_language: \"Cluj-Napoca\""));
            });
        }

        [Test]
        public void GivenALocationWithADefaultLanguage_WhenBuildingTheVictoria3Mod_ThenTheBareStateKeyUsesTheDefault()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("Vic3"));

            string localisation = TextContent.Read(OutputPath(
                "Vic3",
                "localization",
                "english",
                "more_cultural_names_state_names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("STATE_CLUJ_romanian: \"Cluj-Napoca\""));
                Assert.That(localisation, Does.Contain("STATE_CLUJ_english: \"Newport\""));
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheHoI4Mod_ThenNoAdjectiveKeyIsEmitted()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("HOI4"));

            string localisation = TextContent.Read(OutputPath(
                "HOI4",
                "localisation",
                "english",
                "zzz999_more-cultural-names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("romanian_STATE_42:0 \"Cluj-Napoca\""));
                Assert.That(localisation, Does.Not.Contain("_adj"));
            });
        }

        private string OutputPath(string game, params string[] segments)
            => Path.Combine([temporaryDirectory.DirectoryPath, game, "more-cultural-names", .. segments]);

        private static ModBuildRequest BuildRequest(string game)
        {
            ModBuildRequest request = new()
            {
                Game = game,
                LandedTitlesFileName = "00_landed_titles.txt"
            };

            if (string.Equals(game, "CK2", StringComparison.Ordinal))
            {
                request.LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath;
            }

            else
            {
                request.LandedTitlesFilePath = TestPaths.Ck3LandedTitlesFilePath;
            }

            return request;
        }
    }
}
