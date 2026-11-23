using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class EncodingIntegrationTests
    {
        private static readonly string gameDirectoryName = "more-cultural-names";

        private static readonly byte[] utf8ByteOrderMark = [0xEF, 0xBB, 0xBF];

        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(EncodingIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheCk2Mod_ThenTheFilesUseWindows1252WithoutAByteOrderMark()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            string gameDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK2");

            Assert.Multiple(() =>
            {
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(gameDirectoryPath, $"{gameDirectoryName}.mod")),
                    Is.False);
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(
                        gameDirectoryPath,
                        gameDirectoryName,
                        "common",
                        "landed_titles",
                        "00_landed_titles.txt")),
                    Is.False);
                Assert.That(
                    TextContent.ReadWindows1252(Path.Combine(
                        gameDirectoryPath,
                        gameDirectoryName,
                        "localisation",
                        "000_more-cultural-names_landed_titles.csv")),
                    Does.Contain("Cluj-Napoca"));
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheCk3Mod_ThenOnlyLocalisationFilesCarryTheByteOrderMark()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "CK3", gameDirectoryName);

            Assert.Multiple(() =>
            {
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(
                        temporaryDirectory.DirectoryPath,
                        "CK3",
                        $"{gameDirectoryName}.mod")),
                    Is.False);
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(mainDirectoryPath, "descriptor.mod")),
                    Is.False);
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(
                        mainDirectoryPath,
                        "common",
                        "landed_titles",
                        "00_landed_titles.txt")),
                    Is.False);
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(
                        mainDirectoryPath,
                        "localization",
                        "english",
                        $"more-cultural-names_titles_l_english.yml")),
                    Is.True);
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheHoI4Mod_ThenEveryLocalisationFileCarriesTheByteOrderMark()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("HOI4"));

            string localisationDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "HOI4",
                gameDirectoryName,
                "localisation");

            Assert.That(
                PathsStartingWithUtf8ByteOrderMark(localisationDirectoryPath, "*.yml"),
                Has.Count.EqualTo(5));
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheImperatorRomeMod_ThenProvinceNamesLackTheMarkButLocalisationCarriesIt()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("IR"));

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "IR", gameDirectoryName);

            Assert.Multiple(() =>
            {
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(mainDirectoryPath, "common", "province_names", "romanian.txt")),
                    Is.False);
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(
                        mainDirectoryPath,
                        "localization",
                        "more-cultural-names_provincenames_l_english.yml")),
                    Is.True);
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheEuropaUniversalis5Mod_ThenMetadataLacksTheMarkButLocalisationCarriesIt()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("EU5"));

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "EU5", gameDirectoryName);

            Assert.Multiple(() =>
            {
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Is.False);
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(
                        mainDirectoryPath,
                        "main_menu",
                        "localization",
                        "english",
                        "replace",
                        "more_cultural_names_location_names_l_english.yml")),
                    Is.True);
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheVictoria3Mod_ThenMetadataLacksTheMarkButScriptsCarryIt()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("Vic3"));

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "Vic3", gameDirectoryName);

            Assert.Multiple(() =>
            {
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Is.False);
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(
                        mainDirectoryPath,
                        "common",
                        "scripted_effects",
                        "more_cultural_names_state_names.txt")),
                    Is.True);
                Assert.That(
                    StartsWithUtf8ByteOrderMark(Path.Combine(
                        mainDirectoryPath,
                        "common",
                        "on_actions",
                        "more_cultural_names_state_names.txt")),
                    Is.True);
            });
        }

        private static bool StartsWithUtf8ByteOrderMark(string filePath)
        {
            byte[] content = TextContent.ReadBytes(filePath);

            if (content.Length < utf8ByteOrderMark.Length)
            {
                return false;
            }

            return content[0] == utf8ByteOrderMark[0] &&
                content[1] == utf8ByteOrderMark[1] &&
                content[2] == utf8ByteOrderMark[2];
        }

        private static IReadOnlyList<string> PathsStartingWithUtf8ByteOrderMark(string directoryPath, string pattern)
        {
            List<string> matchingFilePaths = [];

            foreach (string filePath in Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories))
            {
                if (StartsWithUtf8ByteOrderMark(filePath))
                {
                    matchingFilePaths.Add(filePath);
                }
            }

            return matchingFilePaths;
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
