using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class HoI4ModBuilderIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        private static IEnumerable<string> ExpectedLanguages =>
            ["english", "french", "german", "polish", "spanish"];

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(HoI4ModBuilderIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheHoI4Mod_ThenEveryFileMatchesTheExpectedContent()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest());

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "HOI4");
            string mainDirectoryPath = Path.Combine(outputDirectoryPath, "more-cultural-names");
            string descriptorPath = Path.Combine(mainDirectoryPath, "descriptor.mod");
            string englishLocalisationPath = Path.Combine(
                mainDirectoryPath,
                "localisation",
                "english",
                "zzz999_more-cultural-names_l_english.yml");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.WithoutVersionComment(TextContent.Read(Path.Combine(outputDirectoryPath, "more-cultural-names.mod"))),
                    Is.EqualTo(ExpectedModContent.HoI4MainDescriptorBody));
                Assert.That(
                    TextContent.WithoutVersionComment(TextContent.Read(descriptorPath)),
                    Is.EqualTo(ExpectedModContent.HoI4DescriptorBody));
                Assert.That(
                    TextContent.Read(englishLocalisationPath),
                    Is.EqualTo(ExpectedModContent.HoI4Localisation));
                Assert.That(
                    Directory.GetDirectories(Path.Combine(mainDirectoryPath, "localisation"))
                        .Select(Path.GetFileName)
                        .Order(StringComparer.Ordinal),
                    Is.EqualTo(ExpectedLanguages));
                Assert.That(
                    Directory.GetFiles(Path.Combine(mainDirectoryPath, "localisation"), "*.yml", SearchOption.AllDirectories)
                        .Length,
                    Is.EqualTo(5));
            });
        }

        [Test]
        public void GivenNonNumericStateIdentifiers_WhenBuildingTheHoI4Mod_ThenAnAggregateExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LocationStorePath = TestPaths.NonNumericHoI4LocationsFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<AggregateException>()
                    .With.InnerException.TypeOf<FormatException>());
        }

        [Test]
        public void GivenDuplicateStateIdentifiers_WhenBuildingTheHoI4Mod_ThenAnAggregateExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LocationStorePath = TestPaths.DuplicateHoI4LocationsFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<AggregateException>()
                    .With.InnerException.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenEmptyDataStores_WhenBuildingTheHoI4Mod_ThenHeaderOnlyLocalisationFilesAreEmitted()
        {
            ModBuildRequest request = BuildRequest();
            request.LanguageStorePath = TestPaths.EmptyLanguagesFilePath;
            request.LocationStorePath = TestPaths.EmptyLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "HOI4",
                "more-cultural-names");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localisation", "english", "zzz999_more-cultural-names_l_english.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("english")));
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, "localisation", "spanish", "zzz999_more-cultural-names_l_spanish.yml")),
                    Is.EqualTo(ExpectedModContent.EmptyLocalisationHeader("spanish")));
            });
        }

        [Test]
        public void GivenADependency_WhenBuildingTheHoI4Mod_ThenTheDescriptorDoesNotDeclareIt()
        {
            ModBuildRequest request = BuildRequest();
            request.Dependency = "A Game of Thrones";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "HOI4", "more-cultural-names.mod")),
                Does.Not.Contain("dependencies"));
        }

        private static ModBuildRequest BuildRequest()
            => new()
            {
                Game = "HOI4"
            };
    }
}
