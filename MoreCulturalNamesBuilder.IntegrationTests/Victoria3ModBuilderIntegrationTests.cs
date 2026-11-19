using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class Victoria3ModBuilderIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(Victoria3ModBuilderIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheVictoria3Mod_ThenEveryFileMatchesTheExpectedContent()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest());

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "Vic3", "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "scripted_effects", "more_cultural_names_state_names.txt")),
                    Is.EqualTo(ExpectedModContent.Victoria3StateNamesEffect));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "on_actions", "more_cultural_names_state_names.txt")),
                    Is.EqualTo(ExpectedModContent.Victoria3StateNamesOnActions));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Is.EqualTo(ExpectedModContent.Victoria3MetadataBody));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more_cultural_names_state_names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.Victoria3StateNamesLocalisation));
                Assert.That(
                    Directory.GetFiles(mainDirectoryPath, "*.yml", SearchOption.AllDirectories),
                    Has.Length.EqualTo(11));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheVictoria3Mod_ThenAnnotationsAreOmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "Vic3",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "scripted_effects", "more_cultural_names_state_names.txt")),
                    Is.EqualTo(ExpectedModContent.Victoria3StateNamesEffectWithoutComments));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more_cultural_names_state_names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.Victoria3StateNamesLocalisationWithoutComments));
            });
        }

        [Test]
        public void GivenEmptyDataStores_WhenBuildingTheVictoria3Mod_ThenOnlyTheAssignmentEffectIsEmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.LanguageStorePath = TestPaths.EmptyLanguagesFilePath;
            request.LocationStorePath = TestPaths.EmptyLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "Vic3",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "scripted_effects", "more_cultural_names_state_names.txt")),
                    Is.EqualTo(ExpectedModContent.Victoria3EmptyStateNamesEffect));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more_cultural_names_state_names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
            });
        }

        [Test]
        public void GivenLocationsWithoutNames_WhenBuildingTheVictoria3Mod_ThenOnlyTheEvaluationAndDispatchEffectsAreEmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.LocationStorePath = TestPaths.NamelessLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string effects = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "Vic3",
                "more-cultural-names",
                "common",
                "scripted_effects",
                "more_cultural_names_state_names.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(effects, Does.Contain("more_cultural_names_evaluate_and_assign_state_hub_dynamic_names = {"));
                Assert.That(effects, Does.Not.Contain("state_name_assign = {"));
            });
        }

        [Test]
        public void GivenADependency_WhenBuildingTheVictoria3Mod_ThenTheMetadataStillDeclaresTheMod()
        {
            ModBuildRequest request = BuildRequest();
            request.Dependency = "A Game of Thrones";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "Vic3",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Does.Contain("\"relationships\": []"));
                Assert.That(
                    File.Exists(Path.Combine(mainDirectoryPath, "descriptor.mod")),
                    Is.False);
            });
        }

        [Test]
        public void GivenAMissingLandedTitlesFile_WhenBuildingTheVictoria3Mod_ThenTheBuildSucceeds()
        {
            ModBuildRequest request = BuildRequest();
            request.LandedTitlesFilePath = TestPaths.MissingLandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.Nothing,
                "Victoria 3 uses dynamic effects rather than landed titles.");
        }

        [Test]
        public void GivenACustomModIdentifier_WhenBuildingTheVictoria3Mod_ThenEveryGeneratedFileUsesIt()
        {
            ModBuildRequest request = BuildRequest();
            request.ModId = "cultural-state-names";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "Vic3",
                "cultural-state-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "scripted_effects", "cultural_state_names_state_names.txt")),
                    Does.Contain("cultural_state_names_evaluate_and_assign_state_hub_dynamic_names = {"));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Does.Contain("\"id\": \"cultural-state-names\""));
            });
        }

        private static ModBuildRequest BuildRequest()
            => new()
            {
                Game = "Vic3",
                GameVersion = "1.18.*"
            };
    }
}
