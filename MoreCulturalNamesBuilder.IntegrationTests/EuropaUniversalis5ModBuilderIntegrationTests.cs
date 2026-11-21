using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class EuropaUniversalis5ModBuilderIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(EuropaUniversalis5ModBuilderIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheEuropaUniversalis5Mod_ThenEveryFileMatchesTheExpectedContent()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest());

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "EU5", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Is.EqualTo(ExpectedModContent.EuropaUniversalis5MetadataBody));
                Assert.That(
                    TextContent.Read(Path.Combine(
                        mainDirectoryPath,
                        "main_menu",
                        "localization",
                        "english",
                        "replace",
                        "more_cultural_names_location_names_l_english.yml")),
                    Is.EqualTo($"l_english:{System.Environment.NewLine}{ExpectedModContent.EuropaUniversalis5LocationNames}"));
                Assert.That(
                    Directory.GetFiles(mainDirectoryPath, "*.yml", SearchOption.AllDirectories),
                    Has.Length.EqualTo(11));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheEuropaUniversalis5Mod_ThenAnnotationsAreOmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string filePath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "EU5",
                "more-cultural-names",
                "main_menu",
                "localization",
                "english",
                "replace",
                "more_cultural_names_location_names_l_english.yml");

            Assert.That(
                TextContent.Read(filePath),
                Is.EqualTo($"l_english:{System.Environment.NewLine}{ExpectedModContent.EuropaUniversalis5LocationNamesWithoutComments}"));
        }

        [Test]
        public void GivenEmptyDataStores_WhenBuildingTheEuropaUniversalis5Mod_ThenOnlyHeaderOnlyLocalisationIsEmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.LanguageStorePath = TestPaths.EmptyLanguagesFilePath;
            request.LocationStorePath = TestPaths.EmptyLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string filePath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "EU5",
                "more-cultural-names",
                "main_menu",
                "localization",
                "english",
                "replace",
                "more_cultural_names_location_names_l_english.yml");

            Assert.That(TextContent.Read(filePath), Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
        }

        [Test]
        public void GivenLocationsWithoutNames_WhenBuildingTheEuropaUniversalis5Mod_ThenOnlyHeaderOnlyLocalisationIsEmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.LocationStorePath = TestPaths.NamelessLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string filePath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "EU5",
                "more-cultural-names",
                "main_menu",
                "localization",
                "english",
                "replace",
                "more_cultural_names_location_names_l_english.yml");

            Assert.Multiple(() =>
            {
                Assert.That(TextContent.Read(filePath), Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
                Assert.That(
                    File.Exists(Path.Combine(
                        temporaryDirectory.DirectoryPath,
                        "EU5",
                        "more-cultural-names",
                        ".metadata",
                        "metadata.json")),
                    Is.True);
            });
        }

        [Test]
        public void GivenADependency_WhenBuildingTheEuropaUniversalis5Mod_ThenTheMetadataStillDeclaresTheMod()
        {
            ModBuildRequest request = BuildRequest();
            request.Dependency = "A Game of Thrones";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string metadataPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "EU5",
                "more-cultural-names",
                ".metadata",
                "metadata.json");

            Assert.Multiple(() =>
            {
                Assert.That(TextContent.Read(metadataPath), Does.Contain("\"relationships\": []"));
                Assert.That(File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, "EU5", "more-cultural-names.mod")), Is.False);
            });
        }

        [Test]
        public void GivenAMissingLandedTitlesFile_WhenBuildingTheEuropaUniversalis5Mod_ThenTheBuildSucceeds()
        {
            ModBuildRequest request = BuildRequest();
            request.LandedTitlesFilePath = TestPaths.MissingLandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.Nothing,
                "Europa Universalis V uses localisation keys rather than landed titles.");
        }

        [Test]
        public void GivenACustomModIdentifier_WhenBuildingTheEuropaUniversalis5Mod_ThenEveryGeneratedFileUsesIt()
        {
            ModBuildRequest request = BuildRequest();
            request.ModId = "location-names";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "EU5",
                "location-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    File.Exists(Path.Combine(
                        mainDirectoryPath,
                        "main_menu",
                        "localization",
                        "english",
                        "replace",
                        "location_names_location_names_l_english.yml")),
                    Is.True);
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Does.Contain("\"id\": \"location-names\""));
            });
        }

        private static ModBuildRequest BuildRequest()
            => new()
            {
                Game = "EU5",
                GameVersion = "1.18.*"
            };
    }
}
