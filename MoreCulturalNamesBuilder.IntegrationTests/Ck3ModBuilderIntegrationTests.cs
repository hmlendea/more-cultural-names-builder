using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class Ck3ModBuilderIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(Ck3ModBuilderIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheCk3Mod_ThenEveryFileMatchesTheExpectedContent()
        {
            ModBuildRequest request = BuildRequest();

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK3");
            string mainDirectoryPath = Path.Combine(outputDirectoryPath, "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.WithoutVersionComment(TextContent.Read(Path.Combine(outputDirectoryPath, "more-cultural-names.mod"))),
                    Is.EqualTo(ExpectedModContent.Ck3MainDescriptorBody));
                Assert.That(
                    TextContent.WithoutVersionComment(TextContent.Read(Path.Combine(mainDirectoryPath, "descriptor.mod"))),
                    Is.EqualTo(ExpectedModContent.Ck3InnerDescriptorBody));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(ExpectedModContent.Ck3LandedTitles));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more-cultural-names_titles_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.Ck3DefaultNamesLocalisation));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more-cultural-names_titles_cultural_names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.Ck3DynamicNamesLocalisation));
                Assert.That(
                    Directory.GetFiles(mainDirectoryPath, "*.yml", SearchOption.AllDirectories),
                    Has.Length.EqualTo(18));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheCk3Mod_ThenLanguageAnnotationsAreOmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(ExpectedModContent.Ck3LandedTitlesWithoutComments));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more-cultural-names_titles_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.Ck3DefaultNamesLocalisationWithoutComments));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more-cultural-names_titles_cultural_names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.Ck3DynamicNamesLocalisationWithoutComments));
            });
        }

        [Test]
        public void GivenADependency_WhenBuildingTheCk3Mod_ThenTheDescriptorDoesNotDeclareIt()
        {
            ModBuildRequest request = BuildRequest();
            request.Dependency = "A Game of Thrones";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string descriptorPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "more-cultural-names.mod");

            Assert.That(
                TextContent.Read(descriptorPath),
                Does.Not.Contain("dependencies"));
        }

        [Test]
        public void GivenAMissingLandedTitlesFile_WhenBuildingTheCk3Mod_ThenAFileNotFoundExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LandedTitlesFilePath = TestPaths.MissingLandedTitlesFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void GivenNoLandedTitlesFileName_WhenBuildingTheCk3Mod_ThenAnArgumentNullExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LandedTitlesFileName = null;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenNoLandedTitlesFilePath_WhenBuildingTheCk3Mod_ThenAnArgumentNullExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LandedTitlesFilePath = null;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenEmptyDataStores_WhenBuildingTheCk3Mod_ThenLocalisationFilesContainOnlyTheHeader()
        {
            ModBuildRequest request = BuildRequest();
            request.LanguageStorePath = TestPaths.EmptyLanguagesFilePath;
            request.LocationStorePath = TestPaths.EmptyLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more-cultural-names_titles_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "english", "more-cultural-names_titles_cultural_names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
            });
        }

        private static ModBuildRequest BuildRequest()
            => new()
            {
                Game = "CK3",
                LandedTitlesFilePath = TestPaths.Ck3LandedTitlesFilePath,
                LandedTitlesFileName = "00_landed_titles.txt"
            };
    }
}
