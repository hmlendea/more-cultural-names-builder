using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class LocalisationContentIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(LocalisationContentIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheCk2Mod_ThenTheDefaultAndPerLanguageRowsAreEmitted()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            string localisation = TextContent.ReadWindows1252(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "localisation",
                "000_more-cultural-names_landed_titles.csv"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("c_cluj;Cluj-Napoca;Cluj-Napoca;Cluj-Napoca;;Cluj-Napoca;;;;;;;;;x"));
                Assert.That(localisation, Does.Contain("c_cluj_adj_romanian;Clujean;"));
                Assert.That(localisation, Does.Contain("c_floresti;Floresti;"));
                Assert.That(localisation, Does.Contain("c_newport;Newport;"));
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheCk2Mod_ThenCulturalNamesAreInjectedIntoTheLandedTitles()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            string landedTitles = TextContent.ReadWindows1252(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(landedTitles, Does.Contain("english = \"Newport\" # Language=English"));
                Assert.That(landedTitles, Does.Contain("romanian = \"Cluj-Napoca\" # Language=Romanian # Praise the Sun!"));
                Assert.That(landedTitles, Does.Contain("has_holder = yes"));
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheCk3Mod_ThenDynamicAndDefaultLocalisationKeysAreEmitted()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK3", "more-cultural-names");
            string englishDirectoryPath = Path.Combine(mainDirectoryPath, "localization", "english");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Does.Contain("name_list_romanian = cn_Cluj-Napoca_romanian"));
                Assert.That(
                    TextContent.Read(Path.Combine(englishDirectoryPath, "more-cultural-names_titles_l_english.yml")),
                    Does.Contain("c_cluj:0 \"Cluj-Napoca\""));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        englishDirectoryPath,
                        "more-cultural-names_titles_cultural_names_l_english.yml")),
                    Does.Contain("cn_Cluj-Napoca_romanian:0 \"Cluj-Napoca\""));
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheCk3Mod_ThenNonLatinAndDiacriticNamesArePreserved()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            string titles = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "more-cultural-names",
                "localization",
                "english",
                "more-cultural-names_titles_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(titles, Does.Contain("c_floresti:0 \"Florești\""));
                Assert.That(titles, Does.Contain("c_cupisan:0 \"Çupișan\""));
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheHoI4Mod_ThenStatesAndVictoryPointsAreDistinguished()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("HOI4"));

            string localisation = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "HOI4",
                "more-cultural-names",
                "localisation",
                "english",
                "zzz999_more-cultural-names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("english_STATE_16:0 \"Newport\""));
                Assert.That(localisation, Does.Contain("english_VICTORY_POINTS_32:0 \"Newport\""));
                Assert.That(localisation, Does.Contain("romanian_STATE_4:0 \"Floreşti\""));
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheImperatorRomeMod_ThenProvinceFilesAndKeysAreEmitted()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("IR"));

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "IR", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "province_names", "romanian.txt")),
                    Does.Contain("42 = PROV42_romanian"));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "localization",
                        "more-cultural-names_provincenames_l_english.yml")),
                    Does.Contain("PROV8_english:0 \"Newport\""));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "localization",
                        "more-cultural-names_provincenames_l_english.yml")),
                    Does.Contain("PROV4:0 \"Floresti\""));
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheEuropaUniversalis5Mod_ThenQualifiedAndDefaultKeysAreEmitted()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("EU5"));

            string localisation = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "EU5",
                "more-cultural-names",
                "main_menu",
                "localization",
                "english",
                "replace",
                "more_cultural_names_location_names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain(" cluj.romanian_language: \"Cluj-Napoca\""));
                Assert.That(localisation, Does.Contain(" cluj: \"Cluj-Napoca\""));
                Assert.That(localisation, Does.Contain(" floresti: \"Florești\""));
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheVictoria3Mod_ThenStateAndHubAssignmentsAreEmitted()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("Vic3"));

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "Vic3", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "common",
                        "scripted_effects",
                        "more_cultural_names_state_names.txt")),
                    Does.Contain("set_state_name = STATE_CLUJ_romanian"));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "common",
                        "scripted_effects",
                        "more_cultural_names_state_names.txt")),
                    Does.Contain("set_hub_name = { type = city name = HUB_NAME_STATE_CLUJ_city_english }"));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "on_actions", "more_cultural_names_state_names.txt")),
                    Does.Contain("on_state_owner_change = {"));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheImperatorRomeMod_ThenOnlyLocationCommentsRemain()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "IR", "more-cultural-names");
            string provinceNames = TextContent.Read(Path.Combine(mainDirectoryPath, "common", "province_names", "romanian.txt"));
            string localisation = TextContent.Read(Path.Combine(
                mainDirectoryPath,
                "localization",
                "more-cultural-names_provincenames_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(provinceNames, Does.Not.Contain("Language="));
                Assert.That(provinceNames, Does.Contain("# Praise the Sun!"));
                Assert.That(localisation, Does.Not.Contain("Language="));
                Assert.That(localisation, Does.Contain("# Praise the Sun!"));
            });
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
