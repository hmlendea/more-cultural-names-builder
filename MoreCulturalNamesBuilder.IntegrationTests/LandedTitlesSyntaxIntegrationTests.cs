using System;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class LandedTitlesSyntaxIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(LandedTitlesSyntaxIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenAnUnbalancedLandedTitlesFile_WhenBuildingTheCk2Mod_ThenTheContentIsPreservedVerbatim()
        {
            string landedTitlesFilePath = WriteLandedTitles(
                "c_unbalanced = {\n    romanian = \"X\"\n");

            ModBuildRunner.Run(
                temporaryDirectory.DirectoryPath,
                BuildRequest(landedTitlesFilePath));

            Assert.That(
                TextContent.ReadWindows1252(OutputLandedTitlesPath("CK2")),
                Is.EqualTo("c_unbalanced = {\n    romanian = \"X\""));
        }

        [Test]
        public void GivenAByteOrderMarkInTheLandedTitlesFile_WhenBuildingTheCk2Mod_ThenTheNamesAreStillInjected()
        {
            string landedTitlesFilePath = Path.Combine(temporaryDirectory.DirectoryPath, "bom_landed_titles.txt");
            byte[] content = [0xEF, 0xBB, 0xBF, .. Encoding.UTF8.GetBytes("c_cluj = {\n}\n")];

            File.WriteAllBytes(landedTitlesFilePath, content);

            ModBuildRunner.Run(
                temporaryDirectory.DirectoryPath,
                BuildRequest(landedTitlesFilePath));

            Assert.That(
                TextContent.ReadWindows1252(OutputLandedTitlesPath("CK2")),
                Does.Contain("english = \"Newport\""));
        }

        [Test]
        public void GivenACarriageReturnOnlyLandedTitlesFile_WhenBuildingTheCk2Mod_ThenTheNamesAreStillInjected()
        {
            string landedTitlesFilePath = WriteLandedTitles("c_cluj = {\r}\r");
            ModBuildRequest request = BuildRequest(landedTitlesFilePath);
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.ReadWindows1252(OutputLandedTitlesPath("CK2")),
                Does.Contain("romanian = \"Cluj-Napoca\" # Praise the Sun!"));
        }

        [Test]
        public void GivenACommentOnlyLandedTitlesFile_WhenBuildingTheCk2Mod_ThenAnEmptyFileIsEmitted()
        {
            string landedTitlesFilePath = WriteLandedTitles("# only a comment\n");

            ModBuildRunner.Run(
                temporaryDirectory.DirectoryPath,
                BuildRequest(landedTitlesFilePath));

            Assert.That(TextContent.ReadBytes(OutputLandedTitlesPath("CK2")), Is.Empty);
        }

        [Test]
        public void GivenLandedTitlesWithNoMatchingLocations_WhenBuildingTheCk2Mod_ThenTheTitlesStayEmpty()
        {
            string landedTitlesFilePath = WriteLandedTitles("c_nomatch = {\n}\n");

            ModBuildRunner.Run(
                temporaryDirectory.DirectoryPath,
                BuildRequest(landedTitlesFilePath));

            Assert.That(
                TextContent.ReadWindows1252(OutputLandedTitlesPath("CK2")),
                Is.EqualTo("c_nomatch = {\n}"));
        }

        [Test]
        public void GivenACk3LandedTitlesFile_WhenBuildingTheCk3Mod_ThenTheCk2NamesAreNotInjected()
        {
            ModBuildRunner.Run(
                temporaryDirectory.DirectoryPath,
                new ModBuildRequest
                {
                    Game = "CK3",
                    LandedTitlesFilePath = TestPaths.Ck3LandedTitlesFilePath,
                    LandedTitlesFileName = "00_landed_titles.txt"
                });

            string landedTitles = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(landedTitles, Does.Contain("cn_Cluj-Napoca_romanian"));
                Assert.That(landedTitles, Does.Not.Contain("romanian = \"Cluj-Napoca\" #"));
            });
        }

        private string WriteLandedTitles(string content)
        {
            string filePath = Path.Combine(temporaryDirectory.DirectoryPath, Guid.NewGuid().ToString("N"));

            File.WriteAllText(filePath, content, new UTF8Encoding(true));

            return filePath;
        }

        private string OutputLandedTitlesPath(string game)
            => Path.Combine(
                temporaryDirectory.DirectoryPath,
                game,
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt");

        private static ModBuildRequest BuildRequest(string landedTitlesFilePath)
            => new()
            {
                Game = "CK2",
                LandedTitlesFilePath = landedTitlesFilePath,
                LandedTitlesFileName = "00_landed_titles.txt"
            };
    }
}
