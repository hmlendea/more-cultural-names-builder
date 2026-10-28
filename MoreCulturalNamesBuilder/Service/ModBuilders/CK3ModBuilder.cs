using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

using NuciDAL.Repositories;

using NuciExtensions;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class CK3ModBuilder(
        ILocalisationFetcher localisationFetcher,
        INameNormaliser nameNormaliser,
        IFileRepository<LanguageDataObject> languageRepository,
        IFileRepository<LocationDataObject> locationRepository,
        Settings settings) : CK2ModBuilder(localisationFetcher, nameNormaliser, languageRepository, locationRepository, settings)
    {
        private readonly object localisationOrderingLock = new();

        private IDictionary<string, IEnumerable<Localisation>>? localisationsOrderedByLanguageId;

        protected override string LocalisationDirectoryName => "localization";

        protected override IEnumerable<string> ForbiddenTokensForPreviousLine => ["allow", "limit", "trigger"];

        protected override IEnumerable<string> ForbiddenTokensForNextLine => ["has_holder"];

        protected override string GenerateMainDescriptorContent()
            => GenerateDescriptorContent() + Environment.NewLine +
                $"path=\"mod/{Settings.Mod.Id}\"";

        protected override string GenerateDescriptorContent()
            => $"# Version {Settings.Mod.Version} ({DateTime.Now})" + Environment.NewLine +
                $"name=\"{Settings.Mod.Name}\"" + Environment.NewLine +
                $"version=\"{Settings.Mod.Version}\"" + Environment.NewLine +
                $"supported_version=\"{Settings.Mod.GameVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Culture\"" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"    \"Map\"" + Environment.NewLine +
                $"    \"Translation\"" + Environment.NewLine +
                $"}}";

        protected override string ReadLandedTitlesFile()
            => File.ReadAllText(Settings.Input.LandedTitlesFilePath!);

        protected override void WriteLandedTitlesFile(string filePath, string content)
            => File.WriteAllText(filePath, CK3CulturalNameBlockMerger.Merge(content));

        protected override string CleanLandedTitlesFile(string content) => content;

        protected override string? GetTitleLocalisationsContent(string line, string gameId)
        {
            EnsureLocalisationsOrderedByLanguageId();

            IEnumerable<Localisation> titleLocalisations = localisationsOrderedByLanguageId!.TryGetValue(gameId);

            if (EnumerableExt.IsNullOrEmpty(titleLocalisations))
            {
                return null;
            }

            string indentation1 = GetLeadingWhitespace(line) + "    ";
            string indentation2 = indentation1 + "    ";
            List<string> lines = [$"{indentation1}cultural_names = {{"];

            foreach (Localisation localisation in titleLocalisations)
            {
                string lineToAdd =
                    $"{indentation2}name_list_{localisation.LanguageGameId} = {GetDynamicLocalisationKey(localisation)}" +
                    $" # {NameNormaliser.ToCK3Charset(localisation.Name)}";

                if (Settings.Output.AreVerboseCommentsEnabled)
                {
                    lineToAdd += $" # Language={localisation.LanguageId}";
                }

                if (!string.IsNullOrWhiteSpace(localisation.Comment))
                {
                    lineToAdd += $" # {NameNormaliser.ToCK3Charset(localisation.Comment)}";
                }

                lines.Add(lineToAdd);
            }

            lines.Add($"{indentation1}}}");

            return string.Join(Environment.NewLine, lines);
        }

        protected override void CreateLocalisationFiles(string localisationDirectoryPath)
        {
            string defaultLocalisationsFileContent = GenerateDefaultNamesLocalisationFileContent();
            string dynamicLocalisationsFileContent = GenerateDynamicNamesLocalisationFileContent();

            List<string> localisationLanguages =
            [
                "english",
                "french",
                "german",
                "polish",
                "spanish",
                "simp_chinese",
                "russian",
                "korean",
                "japanese",
            ];

            Parallel.ForEach(localisationLanguages, fileLanguage => CreateLocalisationFile(
                localisationDirectoryPath,
                "titles",
                fileLanguage,
                defaultLocalisationsFileContent));

            Parallel.ForEach(localisationLanguages, fileLanguage => CreateLocalisationFile(
                localisationDirectoryPath,
                "titles_cultural_names",
                fileLanguage,
                dynamicLocalisationsFileContent));
        }

        protected override void CreateDescriptorFiles()
        {
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{Settings.Mod.Id}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id, "descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }

        private string GenerateDefaultNamesLocalisationFileContent()
        {
            List<string> lines = [];
            object lineCollectionLock = new();
            Dictionary<string, Dictionary<string, Localisation>> localisationsByGameIdAndLanguage =
                BuildLocalisationsByGameIdAndLanguage();

            Parallel.ForEach(
                locations.Values,
                () => new List<string>(),
                (location, _, localLines) =>
                {
                    foreach (GameId gameId in location.GameIds.Where(gameId => string.Equals(gameId.Game, Settings.Mod.Game)))
                    {
                        if (string.IsNullOrWhiteSpace(gameId.DefaultNameLanguageId))
                        {
                            continue;
                        }

                        if (!localisationsByGameIdAndLanguage.TryGetValue(gameId.Id, out Dictionary<string, Localisation>? localisationsByLanguage))
                        {
                            continue;
                        }

                        if (!localisationsByLanguage.TryGetValue(gameId.DefaultNameLanguageId, out Localisation? defaultLocalisation))
                        {
                            continue;
                        }

                        if (defaultLocalisation is null)
                        {
                            continue;
                        }

                        localLines.Add(GenerateLocationLocalisationLine(
                            defaultLocalisation.GameId,
                            defaultLocalisation.Name,
                            defaultLocalisation));

                        if (!string.IsNullOrWhiteSpace(defaultLocalisation.Adjective))
                        {
                            localLines.Add(GenerateLocationLocalisationLine(
                                $"{defaultLocalisation.GameId}_adj",
                                defaultLocalisation.Adjective,
                                defaultLocalisation));
                        }
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

            lines.Sort();

            return string.Join(
                Environment.NewLine,
                lines);
        }

        private Dictionary<string, Dictionary<string, Localisation>> BuildLocalisationsByGameIdAndLanguage()
        {
            Dictionary<string, Dictionary<string, Localisation>> indexedLocalisations = [];

            foreach (KeyValuePair<string, IEnumerable<Localisation>> localisationEntry in localisations)
            {
                Dictionary<string, Localisation> indexedLocalisationsForGameId = [];

                foreach (Localisation localisation in localisationEntry.Value)
                {
                    if (!indexedLocalisationsForGameId.ContainsKey(localisation.LanguageId))
                    {
                        indexedLocalisationsForGameId[localisation.LanguageId] = localisation;
                    }
                }

                indexedLocalisations[localisationEntry.Key] = indexedLocalisationsForGameId;
            }

            return indexedLocalisations;
        }

        private string GenerateDynamicNamesLocalisationFileContent()
        {
            List<string> lines = [];
            object lineCollectionLock = new();

            List<Localisation> uniqueLocalisations = [.. localisations
                .SelectMany(localisationEntry => localisationEntry.Value)
                .GroupBy(localisation => $"{localisation.LanguageGameId}_{NameNormaliser.ToCK3Charset(localisation.Name)}")
                .Select(group => group.OrderBy(localisation => localisation.Id).First())];

            Parallel.ForEach(
                uniqueLocalisations,
                () => new List<string>(),
                (localisation, _, localLines) =>
                {
                    localLines.Add(GenerateLocationLocalisationLine(
                        $"cn_{localisation.Id}_{localisation.LanguageGameId}",
                        localisation.Name,
                        localisation));

                    if (!string.IsNullOrWhiteSpace(localisation.Adjective))
                    {
                        localLines.Add(GenerateLocationLocalisationLine(
                            $"cn_{localisation.Id}_{localisation.LanguageGameId}_adj",
                            localisation.Adjective,
                            localisation));
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

            lines.Sort();

            return string.Join(
                Environment.NewLine,
                lines);
        }

        private string GetDynamicLocalisationKey(Localisation localisation)
        {
            Localisation canonicalLocalisation = localisations
                .SelectMany(localisationEntry => localisationEntry.Value)
                .Where(candidate =>
                    candidate.LanguageGameId.Equals(localisation.LanguageGameId) &&
                    NameNormaliser.ToCK3Charset(candidate.Name)
                        .Equals(NameNormaliser.ToCK3Charset(localisation.Name)))
                .OrderBy(candidate => candidate.Id)
                .First();

            return $"cn_{canonicalLocalisation.Id}_{canonicalLocalisation.LanguageGameId}";
        }

        private void EnsureLocalisationsOrderedByLanguageId()
        {
            lock (localisationOrderingLock)
            {
                localisationsOrderedByLanguageId ??= localisations
                    .ToDictionary(
                        localisationEntry => localisationEntry.Key,
                        localisationEntry =>
                            (IEnumerable<Localisation>)[.. localisationEntry.Value.OrderBy(localisation => localisation.LanguageId)]);
            }
        }

        private void CreateLocalisationFile(string localisationDirectoryPath, string fileLabel, string language, string content)
        {
            string languageDirectoryPath = Path.Combine(localisationDirectoryPath, language);
            string fileContent = $"l_{language}:{Environment.NewLine}{content}";
            string fileName = $"{Settings.Mod.Id}_{fileLabel}_l_{language}.yml";
            string filePath = Path.Combine(languageDirectoryPath, fileName);

            Directory.CreateDirectory(languageDirectoryPath);
            File.WriteAllText(filePath, fileContent, Encoding.UTF8);
        }

        private string GenerateLocationLocalisationLine(string key, string value, Localisation localisation)
        {
            string line =
                $" {key}:0 " +
                $"\"{NameNormaliser.ToCK3Charset(value)}\"";

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
    }
}
