using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class CrossBuilderInvariantIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(CrossBuilderInvariantIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2", "for CK2")]
        [TestCase("CK3", "supported_version")]
        [TestCase("HOI4", "supported_version")]
        [TestCase("IR", "supported_version")]
        [TestCase("EU5", "supported_game_version")]
        [TestCase("Vic3", "supported_game_version")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenTheDescriptorDeclaresTheGameVersion(
            string game,
            string versionMarker)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            string descriptor = ReadDescriptor(game);

            Assert.Multiple(() =>
            {
                Assert.That(descriptor, Does.Contain(versionMarker));
                Assert.That(descriptor, Does.Contain("1.18.*"));
            });
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenTheDescriptorDeclaresTheModName(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            Assert.That(ReadDescriptor(game), Does.Contain("More Cultural Names"));
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheHoI4Mod_ThenStatesAndCitiesAreBothEmitted()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("HOI4"));

            string localisation = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "HOI4",
                "more-cultural-names",
                "localisation",
                "english",
                "zzz999_more-cultural-names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation.Split('\n').Count(line => line.Contains("_STATE_", StringComparison.Ordinal)), Is.EqualTo(10));
                Assert.That(localisation.Split('\n').Count(line => line.Contains("_VICTORY_POINTS_", StringComparison.Ordinal)), Is.EqualTo(10));
            });
        }

        [TestCase("CK3")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingTheMod_ThenEveryLocaleFileForAVariantIsIdentical(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            string[] localisationFiles = [.. Directory
                .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*.yml", SearchOption.AllDirectories)];

            Assert.Multiple(() =>
            {
                Assert.That(localisationFiles, Is.Not.Empty);

                foreach (string localisationFile in localisationFiles)
                {
                    Assert.That(
                        TextContent.Read(localisationFile).Split('\n').Length,
                        Is.GreaterThan(1),
                        localisationFile);
                }
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingEveryStructuralMod_ThenTheRootDescriptorDiffersOnlyByTheTags()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("HOI4"));
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("IR"));

            string ck3Descriptor = TextContent.WithoutVersionComment(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "CK3", "more-cultural-names.mod")));
            string hoI4Descriptor = TextContent.WithoutVersionComment(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "HOI4", "more-cultural-names.mod")));
            string imperatorDescriptor = TextContent.WithoutVersionComment(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "IR", "more-cultural-names.mod")));

            Assert.Multiple(() =>
            {
                Assert.That(hoI4Descriptor, Is.EqualTo(imperatorDescriptor));
                Assert.That(hoI4Descriptor, Does.Not.Contain("\"Culture\""));
                Assert.That(ck3Descriptor, Does.Contain("\"Culture\""));
            });
        }

        private string ReadDescriptor(string game)
            => TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                game,
                ResolveDescriptorRelativePath(game)));

        private static string ResolveDescriptorRelativePath(string game)
        {
            if (string.Equals(game, "EU5", StringComparison.Ordinal) ||
                string.Equals(game, "Vic3", StringComparison.Ordinal))
            {
                return Path.Combine("more-cultural-names", ".metadata", "metadata.json");
            }

            return "more-cultural-names.mod";
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
