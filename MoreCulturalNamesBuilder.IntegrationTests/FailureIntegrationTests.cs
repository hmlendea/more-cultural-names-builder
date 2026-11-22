using System;
using System.IO;

using NuciDAL.Repositories;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class FailureIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(FailureIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenAMissingLanguageStore_WhenBuildingAnyMod_ThenAFileNotFoundExceptionIsThrown(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LanguageStorePath = TestPaths.MissingStoreFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<FileNotFoundException>());
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenAMissingLocationStore_WhenBuildingAnyMod_ThenAFileNotFoundExceptionIsThrown(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LocationStorePath = TestPaths.MissingStoreFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<FileNotFoundException>());
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenAMalformedLanguageStore_WhenBuildingAnyMod_ThenAnInvalidOperationExceptionIsThrown(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LanguageStorePath = TestPaths.MalformedLanguagesFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenAMalformedLocationStore_WhenBuildingAnyMod_ThenAnInvalidOperationExceptionIsThrown(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LocationStorePath = TestPaths.MalformedLocationsFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<InvalidOperationException>());
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenDuplicateLanguageIdentifiers_WhenBuildingAnyMod_ThenADuplicateEntityExceptionIsThrown(
            string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LanguageStorePath = TestPaths.DuplicateLanguagesFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<DuplicateEntityException>());
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenDuplicateLocationIdentifiers_WhenBuildingAnyMod_ThenADuplicateEntityExceptionIsThrown(
            string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LocationStorePath = TestPaths.DuplicateLocationsFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<DuplicateEntityException>());
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenAMissingOutputDirectory_WhenRunningAnyModBuild_ThenADirectoryNotFoundExceptionIsThrown(
            string game)
        {
            string blockingFilePath = Path.Combine(temporaryDirectory.DirectoryPath, $"{game}-blocking-file");
            File.WriteAllText(blockingFilePath, "not a directory");

            Assert.That(
                () => ModBuildRunner.Run(blockingFilePath, BuildRequest(game)),
                Throws.TypeOf<DirectoryNotFoundException>());
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenAnAbsentLandedTitlesFile_WhenBuildingTheStructurallyRelevantMod_ThenOnlyHeadlessBuildersSucceed(
            string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LandedTitlesFilePath = TestPaths.MissingLandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            if (string.Equals(game, "CK2", StringComparison.Ordinal) ||
                string.Equals(game, "CK3", StringComparison.Ordinal))
            {
                Assert.That(
                    () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                    Throws.TypeOf<FileNotFoundException>());
            }

            else
            {
                Assert.That(
                    () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                    Throws.Nothing);
                Assert.That(
                    Directory.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game)),
                    Is.True);
            }
        }

        [TestCase("CK2", "more-cultural-names.mod")]
        [TestCase("CK3", "more-cultural-names.mod")]
        [TestCase("HOI4", "more-cultural-names.mod")]
        [TestCase("IR", "more-cultural-names.mod")]
        [TestCase("EU5", "more-cultural-names/.metadata/metadata.json")]
        [TestCase("Vic3", "more-cultural-names/.metadata/metadata.json")]
        public void GivenANonCanonicalGameVersion_WhenBuildingAnyMod_ThenTheDescriptorDeclaresIt(
            string game,
            string descriptorRelativePath)
        {
            ModBuildRequest request = BuildRequest(game);
            request.GameVersion = "9.9.*";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, game, descriptorRelativePath)),
                Does.Contain("9.9.*"));
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
