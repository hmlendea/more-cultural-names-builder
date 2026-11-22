using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ModBuilderStructureIntegrationTests
    {
        private static readonly string[] ck3LocalisationLanguages =
            ["english", "french", "german", "japanese", "korean", "polish", "russian", "simp_chinese", "spanish"];

        private static readonly string[] hoi4LocalisationLanguages =
            ["english", "french", "german", "polish", "spanish"];

        private static readonly string[] imperatorRomeLocalisationLanguages =
            ["english", "french", "german", "spanish"];

        private static readonly string[] paradoxLocalisationLanguages =
            ["braz_por", "english", "french", "german", "japanese", "korean", "polish", "russian",
                "simp_chinese", "spanish", "turkish"];

        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(ModBuilderStructureIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenExactlyTheExpectedFilesAreEmitted(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            string gameDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, game);
            string[] generatedRelativePaths = [.. Directory
                .GetFiles(gameDirectoryPath, "*", SearchOption.AllDirectories)
                .Select(filePath => Path.GetRelativePath(gameDirectoryPath, filePath))
                .Order(StringComparer.Ordinal)];

            Assert.That(generatedRelativePaths, Is.EqualTo(ExpectedRelativePaths(game)));
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("EU5")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("Vic3")]
        public void GivenTheCanonicalDataStores_WhenBuildingAnyMod_ThenNoEmptyFileIsEmitted(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            string gameDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, game);

            Assert.That(
                Directory.GetFiles(gameDirectoryPath, "*", SearchOption.AllDirectories)
                    .Where(filePath => TextContent.ReadBytes(filePath).Length == 0),
                Is.Empty);
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

        private static IEnumerable<string> ExpectedRelativePaths(string game)
        {
            string mainDirectoryName = "more-cultural-names";
            List<string> relativePaths = [];

            if (string.Equals(game, "CK2", StringComparison.Ordinal))
            {
                relativePaths.Add($"{mainDirectoryName}.mod");
                relativePaths.Add(Path.Combine(mainDirectoryName, "common", "landed_titles", "00_landed_titles.txt"));
                relativePaths.Add(Path.Combine(
                    mainDirectoryName,
                    "localisation",
                    "000_more-cultural-names_landed_titles.csv"));
            }

            if (string.Equals(game, "CK3", StringComparison.Ordinal))
            {
                relativePaths.Add($"{mainDirectoryName}.mod");
                relativePaths.Add(Path.Combine(mainDirectoryName, "descriptor.mod"));
                relativePaths.Add(Path.Combine(mainDirectoryName, "common", "landed_titles", "00_landed_titles.txt"));

                foreach (string language in ck3LocalisationLanguages)
                {
                    relativePaths.Add(Path.Combine(
                        mainDirectoryName,
                        "localization",
                        language,
                        $"more-cultural-names_titles_l_{language}.yml"));
                    relativePaths.Add(Path.Combine(
                        mainDirectoryName,
                        "localization",
                        language,
                        $"more-cultural-names_titles_cultural_names_l_{language}.yml"));
                }
            }

            if (string.Equals(game, "EU5", StringComparison.Ordinal))
            {
                relativePaths.Add(Path.Combine(mainDirectoryName, ".metadata", "metadata.json"));

                foreach (string language in paradoxLocalisationLanguages)
                {
                    relativePaths.Add(Path.Combine(
                        mainDirectoryName,
                        "main_menu",
                        "localization",
                        language,
                        "replace",
                        $"more_cultural_names_location_names_l_{language}.yml"));
                }
            }

            if (string.Equals(game, "HOI4", StringComparison.Ordinal))
            {
                relativePaths.Add($"{mainDirectoryName}.mod");
                relativePaths.Add(Path.Combine(mainDirectoryName, "descriptor.mod"));

                foreach (string language in hoi4LocalisationLanguages)
                {
                    relativePaths.Add(Path.Combine(
                        mainDirectoryName,
                        "localisation",
                        language,
                        $"zzz999_more-cultural-names_l_{language}.yml"));
                }
            }

            if (string.Equals(game, "IR", StringComparison.Ordinal))
            {
                relativePaths.Add($"{mainDirectoryName}.mod");
                relativePaths.Add(Path.Combine(mainDirectoryName, "descriptor.mod"));
                relativePaths.Add(Path.Combine(mainDirectoryName, "common", "province_names", "english.txt"));
                relativePaths.Add(Path.Combine(mainDirectoryName, "common", "province_names", "romanian.txt"));

                foreach (string language in imperatorRomeLocalisationLanguages)
                {
                    relativePaths.Add(Path.Combine(
                        mainDirectoryName,
                        "localization",
                        $"more-cultural-names_provincenames_l_{language}.yml"));
                }
            }

            if (string.Equals(game, "Vic3", StringComparison.Ordinal))
            {
                relativePaths.Add(Path.Combine(mainDirectoryName, ".metadata", "metadata.json"));
                relativePaths.Add(Path.Combine(
                    mainDirectoryName,
                    "common",
                    "scripted_effects",
                    "more_cultural_names_state_names.txt"));
                relativePaths.Add(Path.Combine(
                    mainDirectoryName,
                    "common",
                    "on_actions",
                    "more_cultural_names_state_names.txt"));

                foreach (string language in paradoxLocalisationLanguages)
                {
                    relativePaths.Add(Path.Combine(
                        mainDirectoryName,
                        "localization",
                        language,
                        $"more_cultural_names_state_names_l_{language}.yml"));
                }
            }

            relativePaths.Sort(StringComparer.Ordinal);

            return relativePaths;
        }
    }
}
