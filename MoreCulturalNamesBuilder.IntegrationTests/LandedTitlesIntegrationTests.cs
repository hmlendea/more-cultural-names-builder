using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class LandedTitlesIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(LandedTitlesIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenACustomLandedTitlesFileName_WhenBuildingTheCk2Mod_ThenOnlyThatFileIsWritten()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LandedTitlesFileName = "aaa_custom_landed_titles.txt";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string landedTitlesDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "common",
                "landed_titles");

            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(landedTitlesDirectoryPath), Has.Length.EqualTo(1));
                Assert.That(File.Exists(Path.Combine(landedTitlesDirectoryPath, "aaa_custom_landed_titles.txt")), Is.True);
            });
        }

        [Test]
        public void GivenTheCk2LandedTitles_WhenBuilding_ThenForbiddenTokenBlocksAreNotAnnotated()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            string landedTitles = TextContent.ReadWindows1252(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(landedTitles, Does.Not.Contain("has_holder = yes\n    english"));
                Assert.That(landedTitles, Does.Not.Contain("allow = {\n    english"));
            });
        }

        [Test]
        public void GivenTheCk2LandedTitles_WhenBuilding_ThenTheUntitledBlockIsLeftEmpty()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            string landedTitles = TextContent.ReadWindows1252(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.That(landedTitles, Does.Contain("not_a_title = {\n}"));
        }

        [Test]
        public void GivenTheCk3LandedTitles_WhenBuilding_ThenRepeatedCulturalNameBlocksAreMerged()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            string landedTitles = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(landedTitles, Does.Contain("h_test = {"));
                Assert.That(landedTitles, Does.Contain("name_list_french = cn_first"));
                Assert.That(landedTitles, Does.Contain("name_list_german = cn_second"));
                Assert.That(landedTitles, Does.Not.Contain("name_list_french = cn_first\n    }\n    cultural_names = {"));
            });
        }

        [Test]
        public void GivenTheCk3LandedTitles_WhenBuilding_ThenNestedBaronyCulturalNamesArePreserved()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            string landedTitles = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(landedTitles, Does.Contain("b_test = {"));
                Assert.That(landedTitles, Does.Contain("name_list_french = cn_barony"));
                Assert.That(landedTitles, Does.Contain("name_list_romanian = cn_barony_romanian"));
            });
        }

        [Test]
        public void GivenTheCk3LandedTitles_WhenBuilding_ThenTheOutputUsesTheDefaultEncodingWithoutAByteOrderMark()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            byte[] content = TextContent.ReadBytes(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "more-cultural-names",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.That(content[0], Is.Not.EqualTo(0xEF));
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
