using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class BuildIsolationIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(BuildIsolationIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenAnExistingOutputDirectory_WhenRebuildingAnyMod_ThenTheGeneratedTreeIsIdentical(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, game, "more-cultural-names");

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);
            byte[] firstTree = TextContent.ReadBytesOfDirectory(mainDirectoryPath);

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);
            byte[] secondTree = TextContent.ReadBytesOfDirectory(mainDirectoryPath);

            Assert.That(secondTree, Is.EqualTo(firstTree));
        }

        [Test]
        public void GivenOneOutputDirectory_WhenBuildingEveryMod_ThenEveryGameTreeCoexists()
        {
            foreach (string game in new[] { "CK2", "CK3", "EU5", "HOI4", "IR", "Vic3" })
            {
                ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));
            }

            Assert.Multiple(() =>
            {
                foreach (string game in new[] { "CK2", "CK3", "EU5", "HOI4", "IR", "Vic3" })
                {
                    Assert.That(
                        Directory.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game, "more-cultural-names")),
                        Is.True,
                        game);
                }
            });
        }

        [Test]
        public void GivenTwoOutputDirectories_WhenBuildingEveryMod_ThenEachBuildIsSelfContained()
        {
            string firstDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "first");
            string secondDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "second");

            foreach (string game in new[] { "CK2", "CK3", "EU5", "HOI4", "IR", "Vic3" })
            {
                ModBuildRunner.Run(firstDirectoryPath, BuildRequest(game));
                ModBuildRunner.Run(secondDirectoryPath, BuildRequest(game));
            }

            foreach (string game in new[] { "CK2", "CK3", "EU5", "HOI4", "IR", "Vic3" })
            {
                Assert.That(
                    GeneratedFiles.FindDifferences(
                        Path.Combine(firstDirectoryPath, game, "more-cultural-names"),
                        Path.Combine(secondDirectoryPath, game, "more-cultural-names")),
                    Is.Empty,
                    game);
            }
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
