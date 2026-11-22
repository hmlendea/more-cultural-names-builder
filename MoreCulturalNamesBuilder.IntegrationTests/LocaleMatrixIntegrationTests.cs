using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class LocaleMatrixIntegrationTests
    {
        private static readonly string[] ck3LocaleDirectoryNames =
            [ "english", "french", "german", "japanese", "korean", "polish", "russian", "simp_chinese", "spanish" ];

        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(LocaleMatrixIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK3", new[] { "english", "french", "german", "japanese", "korean", "polish", "russian", "simp_chinese", "spanish" })]
        [TestCase("EU5", new[] { "braz_por", "english", "french", "german", "japanese", "korean", "polish", "russian", "simp_chinese", "spanish", "turkish" })]
        [TestCase("HOI4", new[] { "english", "french", "german", "polish", "spanish" })]
        [TestCase("IR", new[] { "english", "french", "german", "spanish" })]
        [TestCase("Vic3", new[] { "braz_por", "english", "french", "german", "japanese", "korean", "polish", "russian", "simp_chinese", "spanish", "turkish" })]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenEveryLocaleFileCarriesItsOwnLanguageHeader(
            string game,
            string[] expectedLanguages)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            List<string> headers =
                Directory
                    .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*.yml", SearchOption.AllDirectories)
                    .Order(StringComparer.Ordinal)
                    .Select(ReadHeader)
                    .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(
                    headers,
                    Is.All.StartsWith("l_"),
                    $"Every locale file must declare a language header for {game}.");
                Assert.That(
                    headers.Select(header => header[2..].TrimEnd(':')).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal),
                    Is.EqualTo(expectedLanguages.Order(StringComparer.Ordinal)),
                    $"Every declared locale must be emitted as a file for {game}.");
            });
        }

        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheMod_ThenLanguageAnnotationsAreOmitted(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                Directory
                    .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*.yml", SearchOption.AllDirectories)
                    .Where(filePath => TextContent.Read(filePath).Contains("Language=", StringComparison.Ordinal)),
                Is.Empty);
        }

        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenVerboseCommentsAreEnabled_WhenBuildingTheMod_ThenLanguageAnnotationsArePresent(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            Assert.That(
                Directory
                    .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*.yml", SearchOption.AllDirectories)
                    .Where(filePath => TextContent.Read(filePath).Contains("Language=", StringComparison.Ordinal)),
                Is.Not.Empty,
                $"Every locale file must carry the default-language annotation for {game}.");
        }

        [TestCase("HOI4")]
        public void GivenVerboseCommentsAreEnabled_WhenBuildingTheHoI4Mod_ThenNoLanguageAnnotationIsEmitted(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            Assert.That(
                Directory
                    .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*.yml", SearchOption.AllDirectories)
                    .Where(filePath => TextContent.Read(filePath).Contains("Language=", StringComparison.Ordinal)),
                Is.Empty,
                "The HOI4 localisation format carries no language annotation.");
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingTheMod_ThenAuthorCommentsArePreserved(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            Assert.That(
                Directory
                    .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*", SearchOption.AllDirectories)
                    .Where(filePath => TextContent.Read(filePath).Contains("Praise the Sun!", StringComparison.Ordinal)),
                Is.Not.Empty,
                $"Author-supplied comments must survive the build for {game}.");
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenVerboseCommentsAreDisabled_WhenBuildingTheMod_ThenAuthorCommentsArePreserved(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                Directory
                    .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*", SearchOption.AllDirectories)
                    .Where(filePath => TextContent.Read(filePath).Contains("Praise the Sun!", StringComparison.Ordinal)),
                Is.Not.Empty,
                $"Author-supplied comments must survive comment suppression for {game}.");
        }

        [TestCase("HOI4")]
        public void GivenTheCanonicalDataStores_WhenBuildingTheHoI4Mod_ThenAuthorCommentsAreAbsent(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            Assert.That(
                Directory
                    .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*", SearchOption.AllDirectories)
                    .Where(filePath => TextContent.Read(filePath).Contains("Praise the Sun!", StringComparison.Ordinal)),
                Is.Empty,
                "The HOI4 localisation format carries no author comments.");
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheCk3Mod_ThenEachLocaleDirectoryHasBothTitleVariants()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            string localizationDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "more-cultural-names",
                "localization");

            Assert.Multiple(() =>
            {
                foreach (string language in ck3LocaleDirectoryNames)
                {
                    Assert.That(
                        File.Exists(Path.Combine(localizationDirectoryPath, language, $"more-cultural-names_titles_l_{language}.yml")),
                        Is.True,
                        language);
                    Assert.That(
                        File.Exists(Path.Combine(localizationDirectoryPath, language, $"more-cultural-names_titles_cultural_names_l_{language}.yml")),
                        Is.True,
                        language);
                }
            });
        }

        [TestCase("CK3", "more-cultural-names_titles_l_")]
        [TestCase("EU5", "more_cultural_names_location_names_l_")]
        [TestCase("IR", "more-cultural-names_provincenames_l_")]
        [TestCase("Vic3", "more_cultural_names_state_names_l_")]
        public void GivenVerboseCommentsAreEnabled_WhenBuildingTheMod_ThenEachVariantHasAConsistentBody(string game, string filePrefix)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            List<string> bodies =
                Directory
                    .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), $"*{filePrefix}*.yml", SearchOption.AllDirectories)
                    .Order(StringComparer.Ordinal)
                    .Select(filePath => string.Join('\n', TextContent.Read(filePath).Split('\n').Skip(1)))
                    .ToList();

            Assert.That(
                bodies.Distinct(StringComparer.Ordinal).Count(),
                Is.EqualTo(1),
                $"Every '{filePrefix}*.yml' file of {game} must share an identical body, differing only in the header.");
            Assert.That(
                bodies,
                Is.Not.Empty,
                $"No '{filePrefix}*.yml' file was found for {game}.");
        }

        private static string ReadHeader(string filePath)
        {
            string header = TextContent.Read(filePath).Split('\n')[0].TrimStart('\uFEFF');

            if (header.StartsWith("l_", StringComparison.Ordinal))
            {
                return header;
            }

            return $"l_{header}";
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
