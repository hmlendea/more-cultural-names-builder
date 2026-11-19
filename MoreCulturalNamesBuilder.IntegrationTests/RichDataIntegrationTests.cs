using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class RichDataIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(RichDataIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenTheRichDataStores_WhenBuildingTheCk2Mod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.RichCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(TextContent.ReadExpectedWindows1252("rich/ck2_landed_titles.txt")));
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "localisation", "000_more-cultural-names_landed_titles.csv")),
                    Is.EqualTo(TextContent.ReadExpectedWindows1252("rich/ck2_localisation.csv")));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheRichCk2Mod_ThenTheGoldenFileIsReproduced()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.RichCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string landedTitlesPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt");

            Assert.That(
                TextContent.ReadWindows1252(landedTitlesPath),
                Is.EqualTo(TextContent.ReadExpectedWindows1252("rich/ck2_landed_titles_without_comments.txt")));
        }

        [Test]
        public void GivenTheRichDataStores_WhenBuildingTheCk3Mod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("CK3");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.RichCk3LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK3", "more-cultural-names");
            string englishLocalisationDirectoryPath = Path.Combine(mainDirectoryPath, "localization", "english");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ck3_landed_titles.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(englishLocalisationDirectoryPath, "more-cultural-names_titles_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ck3_titles_l_english.yml")));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        englishLocalisationDirectoryPath,
                        "more-cultural-names_titles_cultural_names_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ck3_cultural_names_l_english.yml")));
                Assert.That(
                    Directory.GetFiles(mainDirectoryPath, "*.yml", SearchOption.AllDirectories),
                    Has.Length.EqualTo(18));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheRichCk3Mod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("CK3");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.RichCk3LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK3", "more-cultural-names");
            string englishLocalisationDirectoryPath = Path.Combine(mainDirectoryPath, "localization", "english");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ck3_landed_titles_without_comments.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(englishLocalisationDirectoryPath, "more-cultural-names_titles_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ck3_titles_l_english_without_comments.yml")));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        englishLocalisationDirectoryPath,
                        "more-cultural-names_titles_cultural_names_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ck3_cultural_names_l_english_without_comments.yml")));
            });
        }

        [Test]
        public void GivenTheRichDataStores_WhenBuildingTheHoI4Mod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("HOI4");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;

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
                    Is.EqualTo(TextContent.ReadExpected("rich/hoi4_l_english.yml")));
                Assert.That(
                    Directory.GetDirectories(Path.Combine(mainDirectoryPath, "localisation"))
                        .Select(Path.GetFileName)
                        .Order(StringComparer.Ordinal),
                    Is.EqualTo(new[] { "english", "french", "german", "polish", "spanish" }));
            });
        }

        [Test]
        public void GivenTheRichDataStores_WhenBuildingTheImperatorRomeMod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "IR", "more-cultural-names");
            string provinceNamesDirectoryPath = Path.Combine(mainDirectoryPath, "common", "province_names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "romanian.txt")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ir_romanian.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "english.txt")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ir_english.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "more-cultural-names_provincenames_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ir_l_english.yml")));
                Assert.That(
                    Directory.GetFiles(provinceNamesDirectoryPath, "*.txt").Length,
                    Is.EqualTo(6));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheRichImperatorRomeMod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "IR", "more-cultural-names");
            string provinceNamesDirectoryPath = Path.Combine(mainDirectoryPath, "common", "province_names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "romanian.txt")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ir_romanian_without_comments.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "english.txt")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ir_english_without_comments.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "more-cultural-names_provincenames_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("rich/ir_l_english_without_comments.yml")));
            });
        }

        [Test]
        public void GivenTheRichDataStores_WhenBuildingTheVictoria3Mod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("Vic3");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "Vic3", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "scripted_effects", "more_cultural_names_state_names.txt")),
                    Is.EqualTo(TextContent.ReadExpected("rich/vic3_state_names.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "on_actions", "more_cultural_names_state_names.txt")),
                    Is.EqualTo(TextContent.ReadExpected("rich/vic3_on_actions.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more_cultural_names_state_names_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("rich/vic3_l_english.yml")));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Is.EqualTo(TextContent.ReadExpected("rich/vic3_metadata.json")));
                Assert.That(
                    Directory.GetDirectories(Path.Combine(mainDirectoryPath, "localization")),
                    Has.Length.EqualTo(11));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheRichVictoria3Mod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("Vic3");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "Vic3", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "scripted_effects", "more_cultural_names_state_names.txt")),
                    Is.EqualTo(TextContent.ReadExpected("rich/vic3_state_names_without_comments.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more_cultural_names_state_names_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("rich/vic3_l_english_without_comments.yml")));
            });
        }

        [Test]
        public void GivenTheFallbackDataStores_WhenBuildingTheCk2Mod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LanguageStorePath = TestPaths.FallbackLanguagesFilePath;
            request.LocationStorePath = TestPaths.FallbackLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.FallbackCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(TextContent.ReadExpectedWindows1252("fallback/ck2_landed_titles.txt")));
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "localisation", "000_more-cultural-names_landed_titles.csv")),
                    Is.EqualTo(TextContent.ReadExpectedWindows1252("fallback/ck2_localisation.csv")));
            });
        }

        [Test]
        public void GivenTheFallbackDataStores_WhenBuildingTheCk3Mod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("CK3");
            request.LanguageStorePath = TestPaths.FallbackLanguagesFilePath;
            request.LocationStorePath = TestPaths.FallbackLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.FallbackCk3LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK3", "more-cultural-names");
            string englishLocalisationDirectoryPath = Path.Combine(mainDirectoryPath, "localization", "english");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(TextContent.ReadExpected("fallback/ck3_landed_titles.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(englishLocalisationDirectoryPath, "more-cultural-names_titles_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("fallback/ck3_titles_l_english.yml")));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        englishLocalisationDirectoryPath,
                        "more-cultural-names_titles_cultural_names_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("fallback/ck3_cultural_names_l_english.yml")));
            });
        }

        [Test]
        public void GivenTheFallbackDataStores_WhenBuildingTheHoI4Mod_ThenTheGoldenFileIsReproduced()
        {
            ModBuildRequest request = BuildRequest("HOI4");
            request.LanguageStorePath = TestPaths.FallbackLanguagesFilePath;
            request.LocationStorePath = TestPaths.FallbackLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "HOI4", "more-cultural-names");

            Assert.That(
                TextContent.Read(Path.Combine(
                    mainDirectoryPath,
                    "localisation",
                    "english",
                    "zzz999_more-cultural-names_l_english.yml")),
                Is.EqualTo(TextContent.ReadExpected("fallback/hoi4_l_english.yml")));
        }

        [Test]
        public void GivenTheFallbackDataStores_WhenBuildingTheImperatorRomeMod_ThenTheGoldenFilesAreReproduced()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.LanguageStorePath = TestPaths.FallbackLanguagesFilePath;
            request.LocationStorePath = TestPaths.FallbackLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "IR", "more-cultural-names");
            string provinceNamesDirectoryPath = Path.Combine(mainDirectoryPath, "common", "province_names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "romanian.txt")),
                    Is.EqualTo(TextContent.ReadExpected("fallback/ir_romanian.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "english.txt")),
                    Is.EqualTo(TextContent.ReadExpected("fallback/ir_english.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "french.txt")),
                    Is.EqualTo(TextContent.ReadExpected("fallback/ir_french.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "german.txt")),
                    Is.EqualTo(TextContent.ReadExpected("fallback/ir_german.txt")));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "more-cultural-names_provincenames_l_english.yml")),
                    Is.EqualTo(TextContent.ReadExpected("fallback/ir_l_english.yml")));
            });
        }

        [Test]
        public void GivenTheFallbackDataStores_WhenBuildingTheVictoria3Mod_ThenTheGoldenFileIsReproduced()
        {
            ModBuildRequest request = BuildRequest("Vic3");
            request.LanguageStorePath = TestPaths.FallbackLanguagesFilePath;
            request.LocationStorePath = TestPaths.FallbackLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "Vic3", "more-cultural-names");

            Assert.That(
                TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more_cultural_names_state_names_l_english.yml")),
                Is.EqualTo(TextContent.ReadExpected("fallback/vic3_l_english.yml")));
        }

        [Test]
        public void GivenUnknownLanguages_WhenBuildingTheCk2Mod_ThenOnlyKnownLanguagesAreEmitted()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LanguageStorePath = TestPaths.UnknownLanguageLanguagesFilePath;
            request.LocationStorePath = TestPaths.UnknownLanguageLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.UnknownLanguageCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(TextContent.ReadExpectedWindows1252("edge/ck2_unknown_landed_titles.txt")));
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "localisation", "000_more-cultural-names_landed_titles.csv")),
                    Is.EqualTo(TextContent.ReadExpectedWindows1252("edge/ck2_unknown_localisation.csv")));
            });
        }

        [Test]
        public void GivenBlankAndPaddedNames_WhenBuildingTheCk2Mod_ThenTheValuesArePreserved()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LocationStorePath = TestPaths.BlankNameLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.BlankNameCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(TextContent.ReadExpectedWindows1252("edge/ck2_blank_landed_titles.txt")));
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "localisation", "000_more-cultural-names_landed_titles.csv")),
                    Is.EqualTo(TextContent.ReadExpectedWindows1252("edge/ck2_blank_localisation.csv")));
            });
        }

        [Test]
        public void GivenADanglingLocationFallback_WhenBuildingTheCk2Mod_ThenTheOwnNameIsStillResolved()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LocationStorePath = TestPaths.DanglingFallbackLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.DanglingFallbackCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(TextContent.ReadExpectedWindows1252("edge/ck2_dangling_landed_titles.txt")));
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "localisation", "000_more-cultural-names_landed_titles.csv")),
                    Is.EqualTo(TextContent.ReadExpectedWindows1252("edge/ck2_dangling_localisation.csv")));
            });
        }

        [Test]
        public void GivenADanglingLanguageFallback_WhenBuildingTheCk2Mod_ThenOnlyTheDirectLanguageIsUsed()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LanguageStorePath = TestPaths.DanglingFallbackLanguagesFilePath;
            request.LocationStorePath = TestPaths.DanglingFallbackLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.DanglingFallbackCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string landedTitlesPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt");

            Assert.That(
                TextContent.ReadWindows1252(landedTitlesPath),
                Is.EqualTo("c_orphan = {\n    english = \"Orphan\" # Language=English\n}"));
        }

        [TestCase("ck2")]
        [TestCase("Ck2")]
        public void GivenANormalisedGameIdentifier_WhenBuildingTheRichCk2Mod_ThenTheCk2TreeIsCreatedWithoutData(
            string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.RichCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.Multiple(() =>
            {
                Assert.That(
                    File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game, $"{request.ModId}.mod")),
                    Is.True);
                Assert.That(
                    File.Exists(Path.Combine(
                        temporaryDirectory.DirectoryPath,
                        game,
                        "more-cultural-names",
                        "common",
                        "landed_titles",
                        "00_landed_titles.txt")),
                    Is.True);
            });
        }

        [Test]
        public void GivenTheCanonicalGameIdentifier_WhenBuildingTheRichCk2Mod_ThenTheLandmarkContentIsIdentical()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.RichCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string landedTitlesPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt");

            Assert.That(
                TextContent.ReadWindows1252(landedTitlesPath),
                Is.EqualTo(TextContent.ReadExpectedWindows1252("rich/ck2_landed_titles.txt")));
        }

        [Test]
        public void GivenTheCanonicalGameIdentifier_WhenBuildingTheRichHoI4Mod_ThenTheLocalisationIsIdentical()
        {
            ModBuildRequest request = BuildRequest("HOI4");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string localisationPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "HOI4",
                "more-cultural-names",
                "localisation",
                "english",
                "zzz999_more-cultural-names_l_english.yml");

            Assert.That(
                TextContent.Read(localisationPath),
                Is.EqualTo(TextContent.ReadExpected("rich/hoi4_l_english.yml")));
        }

        [TestCase("ir")]
        [TestCase("ImperatorRome")]
        [TestCase("IMPERATORROME")]
        public void GivenANormalisedGameIdentifier_WhenBuildingTheRichImperatorRomeMod_ThenTheImperatorTreeIsCreated(
            string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                Directory.Exists(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    game,
                    "more-cultural-names",
                    "common",
                    "province_names")),
                Is.True);
        }

        [Test]
        public void GivenTheCanonicalGameIdentifier_WhenBuildingTheRichImperatorRomeMod_ThenTheProvinceNamesAreIdentical()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string provinceNamesPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "IR",
                "more-cultural-names",
                "common",
                "province_names",
                "romanian.txt");

            Assert.That(
                TextContent.Read(provinceNamesPath),
                Is.EqualTo(TextContent.ReadExpected("rich/ir_romanian.txt")));
        }

        [TestCase("vic3")]
        [TestCase("Victoria3")]
        public void GivenANormalisedGameIdentifier_WhenBuildingTheRichVictoria3Mod_ThenTheMetadataIsIdentical(
            string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string metadataPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                game,
                "more-cultural-names",
                ".metadata",
                "metadata.json");

            Assert.That(
                TextContent.Read(metadataPath),
                Is.EqualTo(TextContent.ReadExpected("rich/vic3_metadata.json")));
        }

        [Test]
        public void GivenTheOutputAlias_WhenBuildingTheRichCk2Mod_ThenTheOutputTreeIsCreated()
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, RichCk2Request()),
                "--output");
            string[] aliasedArguments = TestArguments.With(arguments, "--out", temporaryDirectory.DirectoryPath);

            ModBuildRunner.Run(aliasedArguments);

            Assert.That(
                File.Exists(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    "CK2",
                    "more-cultural-names",
                    "common",
                    "landed_titles",
                    "00_landed_titles.txt")),
                Is.True);
        }

        [Test]
        public void GivenTheVersionAlias_WhenBuildingTheCk2Mod_ThenTheVersionIsDeclaredInTheDescriptor()
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, RichCk2Request()),
                "--version");
            string[] aliasedArguments = TestArguments.With(arguments, "--ver", "2.3.4");

            ModBuildRunner.Run(aliasedArguments);

            string descriptor = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names.mod"));

            Assert.That(descriptor, Does.Contain("# Version 2.3.4"));
        }

        [Test]
        public void GivenTheDependencyAlias_WhenBuildingTheCk2Mod_ThenTheDependencyIsDeclared()
        {
            string[] arguments = TestArguments.Build(temporaryDirectory.DirectoryPath, RichCk2Request());
            string[] aliasedArguments = TestArguments.With(arguments, "--dep", "A Game of Thrones");

            ModBuildRunner.Run(aliasedArguments);

            string descriptor = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names.mod"));

            Assert.That(descriptor, Does.Contain("dependencies = { \"A Game of Thrones\" }"));
        }

        [TestCase("1.11.0")]
        [TestCase("2.0.*")]
        public void GivenAGameVersion_WhenBuildingTheCk2Mod_ThenTheDescriptorDeclaresIt(string gameVersion)
        {
            ModBuildRequest request = RichCk2Request();
            request.GameVersion = gameVersion;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string descriptor = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names.mod"));

            Assert.That(descriptor, Does.Contain($"# for CK2 {gameVersion}"));
        }

        [TestCase("cultural-names")]
        [TestCase("mcn")]
        public void GivenACustomModIdentifier_WhenBuildingTheCk2Mod_ThenEveryPathUsesIt(string modId)
        {
            ModBuildRequest request = RichCk2Request();
            request.ModId = modId;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK2", modId);

            Assert.Multiple(() =>
            {
                Assert.That(
                    File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, "CK2", $"{modId}.mod")),
                    Is.True);
                Assert.That(
                    File.Exists(Path.Combine(mainDirectoryPath, "localisation", $"000_{modId}_landed_titles.csv")),
                    Is.True);
            });
        }

        [Test]
        public void GivenTheRichDataStores_WhenBuildingTheHoI4ModTwice_ThenTheOutputIsDeterministic()
        {
            string firstDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "first");
            string secondDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "second");
            ModBuildRequest request = BuildRequest("HOI4");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;

            ModBuildRunner.Run(firstDirectoryPath, request);
            ModBuildRunner.Run(secondDirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(
                    firstDirectoryPath,
                    "HOI4",
                    "more-cultural-names",
                    "localisation",
                    "english",
                    "zzz999_more-cultural-names_l_english.yml")),
                Is.EqualTo(TextContent.Read(Path.Combine(
                    secondDirectoryPath,
                    "HOI4",
                    "more-cultural-names",
                    "localisation",
                    "english",
                    "zzz999_more-cultural-names_l_english.yml"))));
        }

        private static ModBuildRequest BuildRequest(string game)
            => new()
            {
                Game = game
            };

        private static ModBuildRequest RichCk2Request()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            request.LocationStorePath = TestPaths.RichLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.RichCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            return request;
        }
    }
}
