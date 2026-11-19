using System;
using System.IO;
using System.Linq;

using NuciDAL.Repositories;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ResilienceIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(ResilienceIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenDuplicateHoI4StateIdentifiers_WhenBuildingTheHoI4Mod_ThenAnAggregateExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest("HOI4");
            request.LocationStorePath = TestPaths.DuplicateHoI4StatesFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<AggregateException>()
                    .With.InnerException.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenDuplicateLocationIdentifiers_WhenBuildingTheHoI4Mod_ThenNoLocalisationIsEmitted()
        {
            ModBuildRequest request = BuildRequest("HOI4");
            request.LocationStorePath = TestPaths.DuplicateHoI4LocationsFilePath;

            Assert.That(() => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request), Throws.Exception);
            Assert.That(
                File.Exists(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    "HOI4",
                    "more-cultural-names",
                    "localisation",
                    "english",
                    "zzz999_more-cultural-names_l_english.yml")),
                Is.False);
        }

        [Test]
        public void GivenUnknownLanguages_WhenBuildingTheHoI4Mod_ThenOnlyKnownLanguagesAreEmitted()
        {
            ModBuildRequest request = BuildRequest("HOI4");
            request.LanguageStorePath = TestPaths.UnknownLanguageLanguagesFilePath;
            request.LocationStorePath = TestPaths.UnknownLanguageLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string localisation = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "HOI4",
                "more-cultural-names",
                "localisation",
                "english",
                "zzz999_more-cultural-names_l_english.yml"));

            Assert.That(
                localisation,
                Is.EqualTo("l_english:\n english_STATE_2:0 \"Known\"\n romanian_STATE_2:0 \"Known\""));
        }

        [Test]
        public void GivenUnknownLanguages_WhenBuildingTheImperatorRomeMod_ThenOnlyKnownLanguagesAreEmitted()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.LanguageStorePath = TestPaths.UnknownLanguageLanguagesFilePath;
            request.LocationStorePath = TestPaths.UnknownLanguageLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "IR", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "province_names", "romanian.txt")),
                    Is.EqualTo("romanian = {\n    2 = PROV2_romanian # Known # Language=English\n}"));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "more-cultural-names_provincenames_l_english.yml")),
                    Is.EqualTo(
                        "l_english:\n" +
                        " PROV2_english:0 \"Known\" # Language=English\n" +
                        " PROV2_romanian:0 \"Known\" # Language=English\n" +
                        " PROV2:0 \"Known\" # Language=English"));
            });
        }

        [Test]
        public void GivenADanglingLanguageFallback_WhenBuildingTheHoI4Mod_ThenOnlyTheHeaderIsEmitted()
        {
            ModBuildRequest request = BuildRequest("HOI4");
            request.LanguageStorePath = TestPaths.DanglingFallbackLanguagesFilePath;
            request.LocationStorePath = TestPaths.DanglingFallbackLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    "HOI4",
                    "more-cultural-names",
                    "localisation",
                    "english",
                    "zzz999_more-cultural-names_l_english.yml")),
                Is.EqualTo("l_english:\n"));
        }

        [Test]
        public void GivenADanglingLanguageFallback_WhenBuildingTheImperatorRomeMod_ThenNoProvinceDataIsEmitted()
        {
            ModBuildRequest request = BuildRequest("IR");
            request.LanguageStorePath = TestPaths.DanglingFallbackLanguagesFilePath;
            request.LocationStorePath = TestPaths.DanglingFallbackLocationsFilePath;

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
                    Directory.GetFiles(provinceNamesDirectoryPath, "*.txt")
                        .Where(filePath => !string.Equals(File.ReadAllText(filePath), "english = {\n}", StringComparison.Ordinal)),
                    Is.Empty);
                Assert.That(
                    TextContent.Read(Path.Combine(
                        temporaryDirectory.DirectoryPath,
                        "IR",
                        "more-cultural-names",
                        "localization",
                        "more-cultural-names_provincenames_l_english.yml")),
                    Is.EqualTo("l_english:\n"));
            });
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenAnEmptyLanguageStore_WhenBuildingAnyMod_ThenNoLanguageIsEmitted(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LanguageStorePath = TestPaths.EmptyLanguagesFilePath;
            request.LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                Directory.GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*.yml", SearchOption.AllDirectories)
                    .All(filePath => !TextContent.Read(filePath).Contains("Language=", StringComparison.Ordinal)),
                Is.True);
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenAnEmptyLocationStore_WhenBuildingAnyMod_ThenTheDescriptorIsStillEmitted(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LocationStorePath = TestPaths.EmptyLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game, "more-cultural-names.mod")) ||
                File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game, "more-cultural-names", ".metadata", "metadata.json")),
                Is.True);
        }

        [Test]
        public void GivenAMalformedLocationStore_WhenBuildingTheCk2Mod_ThenAnInvalidOperationExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LocationStorePath = TestPaths.MalformedLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenAStringStorePathForNumericLocations_WhenBuildingTheCk2Mod_ThenTheNamesAreStillResolved()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LocationStorePath = TestPaths.UnknownLanguageLocationsFilePath;
            request.LandedTitlesFilePath = TestPaths.UnknownLanguageCk2LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.Nothing);
        }

        [Test]
        public void GivenDuplicateLocations_WhenBuildingTheImperatorRomeMod_ThenAnExceptionIsNotThrownButTheFirstWins()
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

            Assert.That(provinceNames, Does.Contain("42 = PROV42_romanian"));
            Assert.That(provinceNames, Does.Not.Contain("Dezmir"));
        }

        [Test]
        public void GivenAMissingLanguageStore_WhenBuildingTheVictoria3Mod_ThenAFileNotFoundExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest("Vic3");
            request.LanguageStorePath = TestPaths.MissingStoreFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void GivenDuplicateLanguageIdentifiers_WhenBuildingTheVictoria3Mod_ThenADuplicateEntityExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest("Vic3");
            request.LanguageStorePath = TestPaths.DuplicateLanguagesFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<DuplicateEntityException>());
        }

        private static ModBuildRequest BuildRequest(string game)
            => new()
            {
                Game = game
            };
    }
}
