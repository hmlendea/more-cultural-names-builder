using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class OutputHygieneIntegrationTests
    {
        private static readonly string[] allGames = ["CK2", "CK3", "HOI4", "IR", "EU5", "Vic3"];

        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(OutputHygieneIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenNoFileHasTrailingWhitespace(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            List<string> offenders = [.. GeneratedFilePaths(game)
                .SelectMany(filePath => TextContent.Read(filePath)
                    .Split('\n')
                    .Where(line => line.EndsWith(' ') || line.EndsWith('\t'))
                    .Select(line => $"{Path.GetFileName(filePath)}: '{line}'"))];

            Assert.That(offenders, Is.Empty, $"Generated files for {game} must not carry trailing whitespace.");
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenNoFileContainsATabCharacter(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            Assert.That(
                GeneratedFilePaths(game).Where(filePath => TextContent.Read(filePath).Contains('\t')),
                Is.Empty,
                $"Generated files for {game} must be indented with spaces only.");
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenNoFileContainsACarriageReturn(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            Assert.That(
                GeneratedFilePaths(game).Where(filePath => TextContent.Read(filePath).Contains('\r')),
                Is.Empty,
                $"Generated files for {game} must use line feeds only.");
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenEveryLocalisationFileLacksAFinalNewline(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            string[] localisationFiles = [.. GeneratedFilePaths(game)
                .Where(filePath => filePath.EndsWith(".yml", StringComparison.Ordinal) ||
                    filePath.EndsWith(".csv", StringComparison.Ordinal))];

            Assert.Multiple(() =>
            {
                Assert.That(localisationFiles, Is.Not.Empty, $"No localisation files were generated for {game}.");

                foreach (string localisationFile in localisationFiles)
                {
                    Assert.That(
                        TextContent.Read(localisationFile).EndsWith('\n'),
                        Is.False,
                        localisationFile);
                }
            });
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenNoFileContainsConsecutiveBlankLines(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            List<string> offenders = [];

            foreach (string filePath in GeneratedFilePaths(game))
            {
                string[] lines = TextContent.Read(filePath).Split('\n');

                for (int index = 1; index < lines.Length; index += 1)
                {
                    if (lines[index].Length == 0 && lines[index - 1].Length == 0)
                    {
                        offenders.Add($"{Path.GetFileName(filePath)} line {index}");
                    }
                }
            }

            Assert.That(offenders, Is.Empty, $"Generated files for {game} must not contain consecutive blank lines.");
        }

        private IEnumerable<string> GeneratedFilePaths(string game)
            => Directory.GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*", SearchOption.AllDirectories);

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
