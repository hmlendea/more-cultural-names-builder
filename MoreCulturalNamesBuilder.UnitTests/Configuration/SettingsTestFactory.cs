using System.Collections.Generic;

using MoreCulturalNamesBuilder.Configuration;

namespace MoreCulturalNamesBuilder.UnitTests.Configuration
{
    internal static class SettingsTestFactory
    {
        internal static Settings Create(string game, string outputDirectoryPath)
            => Create(game, outputDirectoryPath, null, null, false, null);

        internal static Settings Create(
            string game,
            string outputDirectoryPath,
            string? landedTitlesFilePath,
            string? landedTitlesFileName,
            bool areVerboseCommentsEnabled,
            string? dependency)
            => Create(
                game,
                outputDirectoryPath,
                landedTitlesFilePath,
                landedTitlesFileName,
                areVerboseCommentsEnabled,
                dependency,
                "1.12.*");

        internal static Settings Create(
            string game,
            string outputDirectoryPath,
            string? landedTitlesFilePath,
            string? landedTitlesFileName,
            bool areVerboseCommentsEnabled,
            string? dependency,
            string gameVersion)
        {
            List<string> arguments =
            [
                "--lang", "languages.xml",
                "--loc", "locations.xml",
                "--output", outputDirectoryPath,
                "--id", "more-cultural-names",
                "--name", "More Cultural Names",
                "--version", "1.0.0",
                "--game", game,
                "--game-version", gameVersion,
                "--verbose", areVerboseCommentsEnabled.ToString().ToLowerInvariant()
            ];

            if (!string.IsNullOrWhiteSpace(landedTitlesFilePath))
            {
                arguments.AddRange(["--landed-titles", landedTitlesFilePath]);
            }

            if (!string.IsNullOrWhiteSpace(landedTitlesFileName))
            {
                arguments.AddRange(["--landed-titles-name", landedTitlesFileName]);
            }

            if (!string.IsNullOrWhiteSpace(dependency))
            {
                arguments.AddRange(["--dependency", dependency]);
            }

            return new([.. arguments]);
        }
    }
}
