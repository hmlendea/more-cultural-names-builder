using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class EdgeCaseIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(EdgeCaseIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenUnknownLanguages_WhenBuildingTheCk3Mod_ThenHeaderOnlyLocalisationIsEmitted()
        {
            ModBuildRequest request = BuildRequest("CK3");
            request.LanguageStorePath = TestPaths.UnknownLanguageLanguagesFilePath;
            request.LocationStorePath = TestPaths.UnknownLanguageLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK3", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "localization",
                        "english",
                        "more-cultural-names_titles_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "localization",
                        "english",
                        "more-cultural-names_titles_cultural_names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
                Assert.That(
                    Directory.GetFiles(mainDirectoryPath, "*.yml", SearchOption.AllDirectories),
                    Has.Length.EqualTo(18));
            });
        }

        [Test]
        public void GivenUnknownLanguages_WhenBuildingTheEuropaUniversalis5Mod_ThenHeaderOnlyLocalisationIsEmitted()
        {
            ModBuildRequest request = BuildRequest("EU5");
            request.LanguageStorePath = TestPaths.UnknownLanguageLanguagesFilePath;
            request.LocationStorePath = TestPaths.UnknownLanguageLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "EU5", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "main_menu",
                        "localization",
                        "english",
                        "replace",
                        "more_cultural_names_location_names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
                Assert.That(
                    File.Exists(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Is.True);
            });
        }

        [Test]
        public void GivenUnknownLanguages_WhenBuildingTheVictoria3Mod_ThenTheDispatchEffectHasNoAssignments()
        {
            ModBuildRequest request = BuildRequest("Vic3");
            request.LanguageStorePath = TestPaths.UnknownLanguageLanguagesFilePath;
            request.LocationStorePath = TestPaths.UnknownLanguageLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "Vic3", "more-cultural-names");
            string effects = TextContent.Read(Path.Combine(
                mainDirectoryPath,
                "common",
                "scripted_effects",
                "more_cultural_names_state_names.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(effects, Does.Contain("more_cultural_names_evaluate_and_assign_state_hub_dynamic_names = {"));
                Assert.That(effects, Does.Not.Contain("_state_name_assign = {"));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "localization",
                        "english",
                        "more_cultural_names_state_names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
            });
        }

        [Test]
        public void GivenLocationsWithoutNames_WhenBuildingTheCk3Mod_ThenNoCulturalNamesAreInjected()
        {
            ModBuildRequest request = BuildRequest("CK3");
            request.LocationStorePath = TestPaths.NamelessLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK3", "more-cultural-names");
            string landedTitles = TextContent.Read(Path.Combine(
                mainDirectoryPath,
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(landedTitles, Does.Not.Contain("cn_Cluj-Napoca"));
                Assert.That(landedTitles, Does.Not.Contain("cn_Newport"));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "localization",
                        "english",
                        "more-cultural-names_titles_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
            });
        }

        [Test]
        public void GivenLocationsWithoutNames_WhenBuildingTheHoI4Mod_ThenOnlyHeaderOnlyLocalisationIsEmitted()
        {
            ModBuildRequest request = BuildRequest("HOI4");
            request.LocationStorePath = TestPaths.NamelessLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "HOI4", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "localisation",
                        "english",
                        "zzz999_more-cultural-names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "localisation",
                        "spanish",
                        "zzz999_more-cultural-names_l_spanish.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("spanish")));
            });
        }

        [Test]
        public void GivenLocationsWithoutNames_WhenBuildingTheImperatorRomeMod_ThenProvinceFilesContainOnlyTheHeader()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.LocationStorePath = TestPaths.NamelessLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string provinceNamesDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "IR",
                "more-cultural-names",
                "common",
                "province_names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "english.txt")),
                    Is.EqualTo("english = {\n}"));
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "romanian.txt")),
                    Is.EqualTo("romanian = {\n}"));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        temporaryDirectory.DirectoryPath,
                        "IR",
                        "more-cultural-names",
                        "localization",
                        "more-cultural-names_provincenames_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
            });
        }

        [Test]
        public void GivenBlankAndPaddedNames_WhenBuildingTheHoI4Mod_ThenTheValuesArePreserved()
        {
            ModBuildRequest request = BuildRequest("HOI4");
            request.LocationStorePath = TestPaths.BlankNameLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string localisation = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "HOI4",
                "more-cultural-names",
                "localisation",
                "english",
                "zzz999_more-cultural-names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("english_STATE_1:0 \"\""));
                Assert.That(localisation, Does.Contain("english_STATE_2:0 \"  Spaced  \""));
                Assert.That(localisation, Does.Contain("romanian_STATE_1:0 \"\""));
            });
        }

        [Test]
        public void GivenBlankAndPaddedNames_WhenBuildingTheImperatorRomeMod_ThenTheValuesArePreserved()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.LocationStorePath = TestPaths.BlankNameLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "IR", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "province_names", "english.txt")),
                    Does.Contain("1 = PROV1_english #"));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "province_names", "english.txt")),
                    Does.Contain("2 = PROV2_english #   Spaced  "));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "localization",
                        "more-cultural-names_provincenames_l_english.yml")),
                    Does.Contain("PROV1:0 \"\" # Language=English"));
            });
        }

        [Test]
        public void GivenDuplicateLocations_WhenBuildingTheImperatorRomeMod_ThenTheFirstNameWins()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.LocationStorePath = TestPaths.DuplicateImperatorRomeLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string provinceNames = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "IR",
                "more-cultural-names",
                "common",
                "province_names",
                "romanian.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(provinceNames, Does.Contain("42 = PROV42_romanian"));
                Assert.That(provinceNames, Does.Not.Contain("Dezmir"));
            });
        }

        [Test]
        public void GivenAnEmptyLocationStore_WhenBuildingTheImperatorRomeMod_ThenOnlyTheProvinceHeaderRemains()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.LocationStorePath = TestPaths.EmptyLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string provinceNamesDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "IR",
                "more-cultural-names",
                "common",
                "province_names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "english.txt")),
                    Is.EqualTo("english = {\n}"));
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "romanian.txt")),
                    Is.EqualTo("romanian = {\n}"));
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
