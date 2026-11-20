using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class CrossBuilderIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(CrossBuilderIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2", "more-cultural-names.mod", "name = \"Cultural Names\"")]
        [TestCase("CK3", "more-cultural-names.mod", "name=\"Cultural Names\"")]
        [TestCase("HOI4", "more-cultural-names.mod", "name=\"Cultural Names\"")]
        [TestCase("IR", "more-cultural-names.mod", "name=\"Cultural Names\"")]
        [TestCase("Vic3", "more-cultural-names/.metadata/metadata.json", "\"name\": \"Cultural Names\"")]
        public void GivenACustomModName_WhenBuildingAnyMod_ThenTheDescriptorDeclaresIt(
            string game,
            string relativeFilePath,
            string expectedFragment)
        {
            ModBuildRequest request = CreateRequest(game);
            request.ModName = "Cultural Names";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, game, relativeFilePath)),
                Does.Contain(expectedFragment));
        }

        [TestCase("CK2", "more-cultural-names.mod", "# Version 2.0.0")]
        [TestCase("CK3", "more-cultural-names.mod", "version=\"2.0.0\"")]
        [TestCase("HOI4", "more-cultural-names.mod", "version=\"2.0.0\"")]
        [TestCase("IR", "more-cultural-names.mod", "version=\"2.0.0\"")]
        [TestCase("Vic3", "more-cultural-names/.metadata/metadata.json", "\"version\": \"2.0.0\"")]
        public void GivenACustomModVersion_WhenBuildingAnyMod_ThenTheDescriptorDeclaresIt(
            string game,
            string relativeFilePath,
            string expectedFragment)
        {
            ModBuildRequest request = CreateRequest(game);
            request.ModVersion = "2.0.0";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, game, relativeFilePath)),
                Does.Contain(expectedFragment));
        }

        [TestCase("CK2", "renamed-mod.mod", "more-cultural-names.mod")]
        [TestCase("CK3", "renamed-mod/descriptor.mod", "more-cultural-names.mod")]
        [TestCase("HOI4", "renamed-mod/descriptor.mod", "more-cultural-names.mod")]
        [TestCase("IR", "renamed-mod/descriptor.mod", "more-cultural-names.mod")]
        [TestCase("Vic3", "renamed-mod/.metadata/metadata.json", "more-cultural-names")]
        public void GivenACustomModIdentifier_WhenBuildingAnyMod_ThenOnlyTheRenamedPathsAreWritten(
            string game,
            string relativeFilePath,
            string obsoleteRelativePath)
        {
            ModBuildRequest request = CreateRequest(game);
            request.ModId = "renamed-mod";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, game);

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(Path.Combine(outputDirectoryPath, relativeFilePath)), Is.True);
                Assert.That(File.Exists(Path.Combine(outputDirectoryPath, obsoleteRelativePath)), Is.False);
            });
        }

        [TestCase("HOI4", "more-cultural-names.mod")]
        [TestCase("IR", "more-cultural-names.mod")]
        public void GivenAbsentLandedTitlesInputs_WhenBuildingAStructuralMod_ThenTheBuildSucceeds(
            string game,
            string relativeFilePath)
        {
            ModBuildRequest request = CreateRequest(game);
            request.LandedTitlesFilePath = TestPaths.MissingLandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.Nothing);

            Assert.That(
                File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game, relativeFilePath)),
                Is.True);
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        public void GivenACustomLandedTitlesFileName_WhenBuildingTheMod_ThenTheFileUsesIt(string game)
        {
            ModBuildRequest request = CreateRequest(game);
            request.LandedTitlesFileName = "custom_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string landedTitlesDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                game,
                "more-cultural-names",
                "common",
                "landed_titles");

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(Path.Combine(landedTitlesDirectoryPath, "custom_landed_titles.txt")), Is.True);
                Assert.That(File.Exists(Path.Combine(landedTitlesDirectoryPath, "00_landed_titles.txt")), Is.False);
            });
        }

        [Test]
        public void GivenAMixedCaseVerboseValue_WhenBuildingTheCk2Mod_ThenLanguageAnnotationsAreOmitted()
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, CreateRequest("CK2")),
                "--verbose");
            string[] mixedCaseArguments = TestArguments.With(arguments, "--verbose", "True");

            ModBuildRunner.Run(mixedCaseArguments);

            string landedTitlesPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt");

            Assert.That(TextContent.ReadWindows1252(landedTitlesPath), Does.Not.Contain("Language="));
        }

        [Test]
        public void GivenAnExistingOutputDirectory_WhenBuildingTheCk2ModTwice_ThenTheOutputIsNotDuplicated()
        {
            ModBuildRequest request = CreateRequest("CK2");

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(ExpectedModContent.Ck2LandedTitles));
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "localisation", "000_more-cultural-names_landed_titles.csv")),
                    Is.EqualTo(ExpectedModContent.Ck2Localisation));
            });
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenTheSameInputs_WhenBuildingAnyModTwice_ThenEveryGeneratedFileIsIdentical(string game)
        {
            string firstOutputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "first");
            string secondOutputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "second");
            ModBuildRequest request = CreateRequest(game);

            ModBuildRunner.Run(firstOutputDirectoryPath, request);
            ModBuildRunner.Run(secondOutputDirectoryPath, request);

            Assert.That(
                GeneratedFiles.FindDifferences(
                    Path.Combine(firstOutputDirectoryPath, game, request.ModId),
                    Path.Combine(secondOutputDirectoryPath, game, request.ModId)),
                Is.Empty);
        }

        [TestCase("ck3")]
        [TestCase("Ck3")]
        [TestCase("hoi4")]
        [TestCase("HoI4")]
        public void GivenANormalisedGameIdentifier_WhenBuildingTheMod_ThenTheOutputTreeIsCreated(string game)
        {
            ModBuildRequest request = CreateRequest(game);

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.Multiple(() =>
            {
                Assert.That(
                    File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game, $"{request.ModId}.mod")),
                    Is.True);
                Assert.That(
                    Directory.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game, request.ModId)),
                    Is.True);
            });
        }

        private static ModBuildRequest CreateRequest(string game)
        {
            ModBuildRequest request = new() { Game = game };

            if (game.StartsWith("CK2", StringComparison.OrdinalIgnoreCase))
            {
                request.LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath;
                request.LandedTitlesFileName = "00_landed_titles.txt";
            }

            if (game.StartsWith("CK3", StringComparison.OrdinalIgnoreCase))
            {
                request.LandedTitlesFilePath = TestPaths.Ck3LandedTitlesFilePath;
                request.LandedTitlesFileName = "00_landed_titles.txt";
            }

            return request;
        }
    }
}
