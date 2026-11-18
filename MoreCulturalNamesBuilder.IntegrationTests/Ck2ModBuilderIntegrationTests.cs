using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class Ck2ModBuilderIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(Ck2ModBuilderIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheCk2Mod_ThenEveryFileMatchesTheExpectedContent()
        {
            ModBuildRequest request = BuildRequest();

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK2");
            string mainDirectoryPath = Path.Combine(outputDirectoryPath, "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.WithoutVersionComment(TextContent.Read(Path.Combine(outputDirectoryPath, "more-cultural-names.mod"))),
                    Is.EqualTo(ExpectedModContent.Ck2DescriptorBody));
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(ExpectedModContent.Ck2LandedTitles));
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "localisation", "000_more-cultural-names_landed_titles.csv")),
                    Is.EqualTo(ExpectedModContent.Ck2Localisation));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheCk2Mod_ThenLanguageAnnotationsAreOmitted()
        {
            ModBuildRequest request = BuildRequest();
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
                Is.EqualTo(ExpectedModContent.Ck2LandedTitlesWithoutComments));
        }

        [Test]
        public void GivenADependency_WhenBuildingTheCk2Mod_ThenTheDependencyIsDeclaredInTheDescriptor()
        {
            ModBuildRequest request = BuildRequest();
            request.Dependency = "A Game of Thrones";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string descriptorPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names.mod");

            Assert.That(
                TextContent.Read(descriptorPath),
                Does.Contain("dependencies = { \"A Game of Thrones\" }"));
        }

        [Test]
        public void GivenEmptyDataStores_WhenBuildingTheCk2Mod_ThenThePristineLandedTitlesFileIsEmittedWithoutLocalisation()
        {
            ModBuildRequest request = BuildRequest();
            request.LanguageStorePath = TestPaths.EmptyLanguagesFilePath;
            request.LocationStorePath = TestPaths.EmptyLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(ExpectedModContent.Ck2PristineLandedTitles));
                Assert.That(
                    File.Exists(Path.Combine(mainDirectoryPath, "localisation", "000_more-cultural-names_landed_titles.csv")),
                    Is.False);
            });
        }

        [Test]
        public void GivenLocationsWithoutNames_WhenBuildingTheCk2Mod_ThenNoCulturalNamesAreInjected()
        {
            ModBuildRequest request = BuildRequest();
            request.LocationStorePath = TestPaths.NamelessLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(mainDirectoryPath, "common", "landed_titles", "00_landed_titles.txt")),
                    Is.EqualTo(ExpectedModContent.Ck2NamelessLandedTitles));
                Assert.That(
                    File.Exists(Path.Combine(mainDirectoryPath, "localisation", "000_more-cultural-names_landed_titles.csv")),
                    Is.False);
            });
        }

        [Test]
        public void GivenAMissingLandedTitlesFile_WhenBuildingTheCk2Mod_ThenAFileNotFoundExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LandedTitlesFilePath = TestPaths.MissingLandedTitlesFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void GivenNoLandedTitlesFileName_WhenBuildingTheCk2Mod_ThenAnArgumentNullExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LandedTitlesFileName = null;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenNoLandedTitlesFilePath_WhenBuildingTheCk2Mod_ThenAnArgumentNullExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LandedTitlesFilePath = null;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenAFailingBuild_WhenBuildingTheCk2Mod_ThenNoLocalisationFileIsEmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.LandedTitlesFileName = null;

            Assert.That(() => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request), Throws.Exception);
            Assert.That(
                File.Exists(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    "CK2",
                    "more-cultural-names",
                    "localisation",
                    "000_more-cultural-names_landed_titles.csv")),
                Is.False);
        }

        private static ModBuildRequest BuildRequest()
            => new()
            {
                Game = "CK2",
                LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath,
                LandedTitlesFileName = "00_landed_titles.txt"
            };
    }
}
