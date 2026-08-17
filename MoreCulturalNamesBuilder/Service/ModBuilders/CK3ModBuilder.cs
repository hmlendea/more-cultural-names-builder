using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NuciDAL.Repositories;
using NuciExtensions;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class CK3ModBuilder(
        ILocalisationFetcher localisationFetcher,
        INameNormaliser nameNormaliser,
        IFileRepository<LanguageEntity> languageRepository,
        IFileRepository<LocationEntity> locationRepository,
        Settings settings) : CK2ModBuilder(localisationFetcher, nameNormaliser, languageRepository, locationRepository, settings)
    {
        protected override string LocalisationDirectoryName => "localization";
        protected override List<string> ForbiddenTokensForPreviousLine => ["allow", "limit", "trigger"];
        protected override List<string> ForbiddenTokensForNextLine => ["has_holder"];

        readonly INameNormaliser nameNormaliser = nameNormaliser;
        IDictionary<string, IEnumerable<Localisation>> localisationsOrderedByLanguageId;

        protected override string GenerateMainDescriptorContent()
            => GenerateDescriptorContent() + Environment.NewLine +
                $"path=\"mod/{Settings.Mod.Id}\"";

        protected override string GenerateDescriptorContent()
            =>  $"# Version {Settings.Mod.Version} ({DateTime.Now})" + Environment.NewLine +
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
            => File.ReadAllText(Settings.Input.LandedTitlesFilePath);

        protected override void WriteLandedTitlesFile(string filePath, string content)
            => File.WriteAllText(filePath, content);

        protected override string DoCleanLandedTitlesFile(string content)
        {
            string nameListsPattern = string.Join('|', languageGameIds.Select(x => $"name_list_{x.Id}"));

            Regex breakInlineRegex = new(
                $"^(\\s*)([ekdcb]_[^\\s]*)\\s*=\\s*\\{{\\s*((" + nameListsPattern + ")\\s*=\\s*[a-zA-Z_-]*)\\s*\\}}",
                RegexOptions.Multiline | RegexOptions.Compiled);
            Regex removeOriginalRegex = new Regex(
                $"^\\s*({nameListsPattern})\\s*=\\s*[a-zA-Z_-]*\\s*\n",
                RegexOptions.Multiline | RegexOptions.Compiled);
            Regex removeEmptyBlockRegex = new Regex(
                "^\\s*cultural_names\\s*=\\s*{\\s*\r*\n\\s*}\\s*\r*\n",
                RegexOptions.Multiline | RegexOptions.Compiled);

            string cleanContent = content;

            cleanContent = breakInlineRegex.Replace(cleanContent, "$1$2 = {\n$1\t$3\n$1}");
            cleanContent = removeOriginalRegex.Replace(cleanContent, string.Empty);
            cleanContent = removeEmptyBlockRegex.Replace(cleanContent, string.Empty);

            return cleanContent;
        }

        protected override string GetTitleLocalisationsContent(string line, string gameId)
        {
            EnsureLocalisationsOrderedByLanguageId();

            IEnumerable<Localisation> titleLocalisations = localisationsOrderedByLanguageId.TryGetValue(gameId);

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
                    $"{indentation2}name_list_{localisation.LanguageGameId} = cn_{localisation.Id}_{localisation.LanguageGameId}" +
                    $" # {nameNormaliser.ToCK3Charset(localisation.Name)}";

                if (Settings.Output.AreVerboseCommentsEnabled)
                {
                    lineToAdd += $" # Language={localisation.LanguageId}";
                }

                if (!string.IsNullOrWhiteSpace(localisation.Comment))
                {
                    lineToAdd += $" # {nameNormaliser.ToCK3Charset(localisation.Comment)}";
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

            List<string> localisationLanguages = ["english", "french", "german", "spanish"];

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

        string GenerateDefaultNamesLocalisationFileContent()
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
                    foreach (GameId gameId in location.GameIds.Where(x => x.Game.Equals(Settings.Mod.Game)))
                    {
                        if (string.IsNullOrWhiteSpace(gameId.DefaultNameLanguageId))
                        {
                            continue;
                        }

                        if (!localisationsByGameIdAndLanguage.TryGetValue(gameId.Id, out Dictionary<string, Localisation> localisationsByLanguage))
                        {
                            continue;
                        }

                        if (!localisationsByLanguage.TryGetValue(gameId.DefaultNameLanguageId, out Localisation defaultLocalisation))
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

        Dictionary<string, Dictionary<string, Localisation>> BuildLocalisationsByGameIdAndLanguage()
        {
            Dictionary<string, Dictionary<string, Localisation>> indexedLocalisations = [];

            foreach ((string gameId, IEnumerable<Localisation> localisationsForGameId) in localisations)
            {
                Dictionary<string, Localisation> indexedLocalisationsForGameId = [];

                foreach (Localisation localisation in localisationsForGameId)
                {
                    if (!indexedLocalisationsForGameId.ContainsKey(localisation.LanguageId))
                    {
                        indexedLocalisationsForGameId[localisation.LanguageId] = localisation;
                    }
                }

                indexedLocalisations[gameId] = indexedLocalisationsForGameId;
            }

            return indexedLocalisations;
        }

        string GenerateDynamicNamesLocalisationFileContent()
        {
            List<string> lines = [];
            object lineCollectionLock = new();

            List<Localisation> uniqueLocalisations = BuildUniqueLocalisations();

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

        void EnsureLocalisationsOrderedByLanguageId()
        {
            if (localisationsOrderedByLanguageId is not null)
            {
                return;
            }

            IDictionary<string, IEnumerable<Localisation>> orderedLocalisationsByLanguageId = localisations
                .ToDictionary(
                    localisationsByGameId => localisationsByGameId.Key,
                    localisationsByGameId =>
                        (IEnumerable<Localisation>)localisationsByGameId.Value
                            .OrderBy(localisation => localisation.LanguageId)
                            .ToArray());

            Interlocked.CompareExchange(
                ref localisationsOrderedByLanguageId,
                orderedLocalisationsByLanguageId,
                null);
        }

        static List<Localisation> BuildUniqueLocalisations(
            IDictionary<string, IEnumerable<Localisation>> localisations)
        {
            HashSet<string> seenLocalisationKeys = [];
            List<Localisation> uniqueLocalisations = [];

            foreach (IEnumerable<Localisation> localisationsForGameId in localisations.Values)
            {
                foreach (Localisation localisation in localisationsForGameId)
                {
                    string localisationKey = $"{localisation.Id}_{localisation.LanguageGameId}";

                    if (!seenLocalisationKeys.Add(localisationKey))
                    {
                        continue;
                    }

                    uniqueLocalisations.Add(localisation);
                }
            }

            return uniqueLocalisations;
        }

        List<Localisation> BuildUniqueLocalisations() => BuildUniqueLocalisations(localisations);

        void CreateLocalisationFile(string localisationDirectoryPath, string fileLabel, string language, string content)
        {
            string fileContent = $"l_{language}:{Environment.NewLine}{content}";
            string fileName = $"{Settings.Mod.Id}_{fileLabel}_l_{language}.yml";
            string filePath = Path.Combine(localisationDirectoryPath, fileName);

            File.WriteAllText(filePath, fileContent, Encoding.UTF8);
        }

        string GenerateLocationLocalisationLine(string key, string value, Localisation localisation)
        {
            string line =
                $" {key}:0 " +
                $"\"{nameNormaliser.ToCK3Charset(value)}\"";

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
