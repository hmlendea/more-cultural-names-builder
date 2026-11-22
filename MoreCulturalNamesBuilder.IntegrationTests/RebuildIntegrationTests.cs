using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class RebuildIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(RebuildIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenAnExistingTree_WhenRebuildingWithADifferentModName_ThenTheOldNameIsReplaced(string game)
        {
            ModBuildRequest firstRequest = BuildRequest(game);
            firstRequest.ModName = "First Name";
            ModBuildRequest secondRequest = BuildRequest(game);
            secondRequest.ModName = "Second Name";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, firstRequest);
            string firstDescriptor = ReadDescriptor(game);

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, secondRequest);
            string secondDescriptor = ReadDescriptor(game);

            Assert.Multiple(() =>
            {
                Assert.That(firstDescriptor, Does.Contain("First Name"));
                Assert.That(secondDescriptor, Does.Contain("Second Name"));
                Assert.That(secondDescriptor, Does.Not.Contain("First Name"));
            });
        }

        [Test]
        public void GivenAnExistingTree_WhenRebuildingTheHoI4ModWithDifferentDataStores_ThenTheContentIsReplaced()
        {
            ModBuildRequest richRequest = BuildRequest("HOI4");
            richRequest.LanguageStorePath = TestPaths.RichLanguagesFilePath;
            richRequest.LocationStorePath = TestPaths.RichLocationsFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("HOI4"));
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, richRequest);

            Assert.That(
                Directory
                    .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, "HOI4"), "*", SearchOption.AllDirectories)
                    .Where(filePath => TextContent.Read(filePath).Contains("Solara", StringComparison.Ordinal)),
                Is.Not.Empty,
                "The richer data stores must supersede the canonical ones on rebuild.");
        }

        private string ReadDescriptor(string game)
        {
            string relativePath = "more-cultural-names.mod";

            if (string.Equals(game, "EU5", StringComparison.Ordinal) ||
                string.Equals(game, "Vic3", StringComparison.Ordinal))
            {
                relativePath = Path.Combine("more-cultural-names", ".metadata", "metadata.json");
            }

            return TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, game, relativePath));
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
