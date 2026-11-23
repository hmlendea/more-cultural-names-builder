using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

using NuciDAL.Repositories;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class EuropaUniversalis5ModBuilder(
        ILocalisationFetcher localisationFetcher,
        INameNormaliser nameNormaliser,
        IFileRepository<LanguageDataObject> languageRepository,
        IFileRepository<LocationDataObject> locationRepository,
        Settings settings) : ModBuilder(languageRepository, locationRepository, settings)
    {
        private static readonly string[] displayLanguages =
        [
            "braz_por",
            "english",
            "french",
            "german",
            "japanese",
            "korean",
            "polish",
            "russian",
            "simp_chinese",
            "spanish",
            "turkish",
        ];

        private IList<string> locationIds = null!;
        private IDictionary<string, IDictionary<string, Localisation>> localisationsByLocationId = null!;
        private IDictionary<string, string> defaultLanguageIdsByLocationId = null!;

        protected override void LoadData()
        {
            ConcurrentDictionary<string, IDictionary<string, Localisation>> concurrentLocalisations = [];
            ConcurrentDictionary<string, string> concurrentDefaultLanguages = [];

            locationIds = [.. locationGameIds
                .Where(gameId => !string.IsNullOrWhiteSpace(gameId.Id))
                .Select(gameId => gameId.Id)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(locationId => locationId, StringComparer.Ordinal)];

            Parallel.ForEach(locationIds, locationId =>
            {
                concurrentLocalisations[locationId] = localisationFetcher
                    .GetGameLocationLocalisations(locationId, Settings.Mod.Game)
                    .ToDictionary(localisation => localisation.LanguageGameId, localisation => localisation);

                string? defaultLanguageId = locationGameIds
                    .FirstOrDefault(gameId => string.Equals(gameId.Id, locationId, StringComparison.Ordinal))
                    ?.DefaultNameLanguageId;

                if (!string.IsNullOrWhiteSpace(defaultLanguageId))
                {
                    concurrentDefaultLanguages[locationId] = defaultLanguageId;
                }
            });

            localisationsByLocationId = concurrentLocalisations;
            defaultLanguageIdsByLocationId = concurrentDefaultLanguages;
        }

        protected override void GenerateFiles()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
            string metadataDirectoryPath = Path.Combine(mainDirectoryPath, ".metadata");
            string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "main_menu", "localization");

            Directory.CreateDirectory(metadataDirectoryPath);
            Directory.CreateDirectory(localisationDirectoryPath);

            CreateMetadataFile(metadataDirectoryPath);
            CreateLocalisationFiles(localisationDirectoryPath);
        }

        private void CreateMetadataFile(string metadataDirectoryPath)
        {
            string filePath = Path.Combine(metadataDirectoryPath, "metadata.json");

            File.WriteAllText(filePath, GenerateMetadataContent(), new UTF8Encoding(false));
        }

        private void CreateLocalisationFiles(string localisationDirectoryPath)
        {
            string content = GenerateLocalisationFileContent();

            Parallel.ForEach(
                displayLanguages,
                language => CreateLocalisationFile(localisationDirectoryPath, language, content));
        }

        private void CreateLocalisationFile(string localisationDirectoryPath, string language, string content)
        {
            string languageDirectoryPath = Path.Combine(localisationDirectoryPath, language, "replace");
            string fileContent = $"l_{language}:{Environment.NewLine}{content}";
            string fileName = $"{GetFilePrefix()}_location_names_l_{language}.yml";
            string filePath = Path.Combine(languageDirectoryPath, fileName);

            Directory.CreateDirectory(languageDirectoryPath);
            File.WriteAllText(filePath, fileContent, new UTF8Encoding(true));
        }

        private string GenerateLocalisationFileContent()
        {
            List<string> lines = [];
            object lineCollectionLock = new();

            Parallel.ForEach(
                locationIds,
                List<string> () => [],
                (locationId, _, localLines) =>
                {
                    IDictionary<string, Localisation> locationLocalisations = localisationsByLocationId[locationId];
                    Localisation? defaultLocalisation = GetDefaultLocalisation(locationId, locationLocalisations);

                    if (defaultLocalisation is not null)
                    {
                        localLines.Add(GenerateLocalisationLine(locationId, defaultLocalisation));
                    }

                    foreach (string languageGameId in locationLocalisations.Keys.OrderBy(id => id, StringComparer.Ordinal))
                    {
                        localLines.Add(GenerateLocalisationLine(
                            $"{locationId}.{languageGameId}",
                            locationLocalisations[languageGameId]));
                    }

                    return localLines;
                },
                localLines =>
                {
                    lock (lineCollectionLock)
                    {
                        lines.AddRange(localLines);
                    }
                });

            lines.Sort(StringComparer.Ordinal);

            return string.Join(Environment.NewLine, lines);
        }

        private string GenerateLocalisationLine(string localisationKey, Localisation localisation)
        {
            string line =
                $" {localisationKey}: " +
                $"\"{nameNormaliser.ToEuropaUniversalis5Charset(localisation.Name)}\"";

            if (Settings.Output.AreVerboseCommentsEnabled)
            {
                line += $" # Language={localisation.LanguageId}";
            }

            if (!string.IsNullOrWhiteSpace(localisation.Comment))
            {
                line += $" # {localisation.Comment}";
            }

            return line;
        }

        private Localisation? GetDefaultLocalisation(
            string locationId,
            IDictionary<string, Localisation> localisations)
        {
            if (!defaultLanguageIdsByLocationId.TryGetValue(locationId, out string? defaultLanguageId))
            {
                return null;
            }

            return localisations.Values.FirstOrDefault(localisation =>
                string.Equals(localisation.LanguageId, defaultLanguageId, StringComparison.Ordinal));
        }

        private string GenerateMetadataContent()
        {
            StringBuilder builder = new();

            builder.Append('{');
            builder.Append(Environment.NewLine);
            builder.Append($"  \"name\": \"{Settings.Mod.Name}\",");
            builder.Append(Environment.NewLine);
            builder.Append($"  \"id\": \"{Settings.Mod.Id}\",");
            builder.Append(Environment.NewLine);
            builder.Append($"  \"version\": \"{Settings.Mod.Version}\",");
            builder.Append(Environment.NewLine);
            builder.Append("  \"game_id\": \"eu5\",");
            builder.Append(Environment.NewLine);
            builder.Append("  \"picture\": \"thumbnail.png\",");
            builder.Append(Environment.NewLine);
            builder.Append($"  \"supported_game_version\": \"{Settings.Mod.GameVersion}\",");
            builder.Append(Environment.NewLine);
            builder.Append($"  \"short_description\": \"{Settings.Mod.Name}\",");
            builder.Append(Environment.NewLine);
            builder.Append("  \"tags\": [");
            builder.Append(Environment.NewLine);
            builder.Append("    \"Culture & Art\",");
            builder.Append(Environment.NewLine);
            builder.Append("    \"Historical\",");
            builder.Append(Environment.NewLine);
            builder.Append("    \"Map\",");
            builder.Append(Environment.NewLine);
            builder.Append("    \"Translation\"");
            builder.Append(Environment.NewLine);
            builder.Append("  ],");
            builder.Append(Environment.NewLine);
            builder.Append("  \"relationships\": [],");
            builder.Append(Environment.NewLine);
            builder.Append("  \"game_custom_data\": {");
            builder.Append(Environment.NewLine);
            builder.Append("    \"multiplayer_synchronized\": true");
            builder.Append(Environment.NewLine);
            builder.Append("  }");
            builder.Append(Environment.NewLine);
            builder.Append('}');

            return builder.ToString();
        }

        private string GetFilePrefix() => Settings.Mod.Id.Replace('-', '_');
    }
}
