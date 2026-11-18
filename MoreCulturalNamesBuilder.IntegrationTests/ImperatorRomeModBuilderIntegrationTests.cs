using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ImperatorRomeModBuilderIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(ImperatorRomeModBuilderIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheImperatorRomeMod_ThenEveryFileMatchesTheExpectedContent()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest());

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "IR");
            string mainDirectoryPath = Path.Combine(outputDirectoryPath, "more-cultural-names");
            string provinceNamesDirectoryPath = Path.Combine(mainDirectoryPath, "common", "province_names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.WithoutVersionComment(TextContent.Read(Path.Combine(outputDirectoryPath, "more-cultural-names.mod"))),
                    Is.EqualTo(ExpectedModContent.ImperatorRomeMainDescriptorBody));
                Assert.That(
                    TextContent.WithoutVersionComment(TextContent.Read(Path.Combine(mainDirectoryPath, "descriptor.mod"))),
                    Is.EqualTo(ExpectedModContent.ImperatorRomeDescriptorBody));
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "romanian.txt")),
                    Is.EqualTo(ExpectedModContent.ImperatorRomeRomanianProvinceNames));
                Assert.That(
                    TextContent.Read(Path.Combine(provinceNamesDirectoryPath, "english.txt")),
                    Is.EqualTo(ExpectedModContent.ImperatorRomeEnglishProvinceNames));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "more-cultural-names_provincenames_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.ImperatorRomeLocalisation));
                Assert.That(
                    Directory.GetFiles(Path.Combine(mainDirectoryPath, "localization"), "*.yml").Length,
                    Is.EqualTo(4));
                Assert.That(
                    Directory.GetFiles(provinceNamesDirectoryPath, "*.txt").Length,
                    Is.EqualTo(2));
            });
        }

        [Test]
        public void GivenNonNumericProvinceIdentifiers_WhenBuildingTheImperatorRomeMod_ThenAnAggregateExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LocationStorePath = TestPaths.NonNumericImperatorRomeLocationsFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<AggregateException>()
                    .With.InnerException.TypeOf<FormatException>());
        }

        [Test]
        public void GivenDuplicateProvinceIdentifiers_WhenBuildingTheImperatorRomeMod_ThenOnlyTheFirstLocationIsEmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.LocationStorePath = TestPaths.DuplicateImperatorRomeLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "IR",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "common", "province_names", "romanian.txt")),
                    Is.EqualTo(ExpectedModContent.ImperatorRomeDuplicateProvinceNames));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "more-cultural-names_provincenames_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.ImperatorRomeDuplicateLocalisation));
            });
        }

        [Test]
        public void GivenEmptyDataStores_WhenBuildingTheImperatorRomeMod_ThenHeaderOnlyLocalisationFilesAreEmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.LanguageStorePath = TestPaths.EmptyLanguagesFilePath;
            request.LocationStorePath = TestPaths.EmptyLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "IR",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localization", "more-cultural-names_provincenames_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
                Assert.That(
                    Directory.GetFiles(Path.Combine(mainDirectoryPath, "common", "province_names"), "*.txt"),
                    Is.Empty);
            });
        }

        [Test]
        public void GivenADependency_WhenBuildingTheImperatorRomeMod_ThenTheDescriptorDoesNotDeclareIt()
        {
            ModBuildRequest request = BuildRequest();
            request.Dependency = "A Game of Thrones";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "IR", "more-cultural-names.mod")),
                Does.Not.Contain("dependencies"));
        }

        private static ModBuildRequest BuildRequest()
            => new()
            {
                Game = "IR"
            };
    }
}
