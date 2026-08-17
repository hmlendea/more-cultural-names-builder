using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NuciDAL.Repositories;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class ImperatorRomeModBuilder(
        ILocalisationFetcher localisationFetcher,
        INameNormaliser nameNormaliser,
        IFileRepository<LanguageEntity> languageRepository,
        IFileRepository<LocationEntity> locationRepository,
        Settings settings) : ModBuilder(languageRepository, locationRepository, settings)
    {
        IDictionary<string, IDictionary<string, Localisation>> localisations;
        IDictionary<string, GameId> locationGameIdsById;

        readonly ILocalisationFetcher localisationFetcher = localisationFetcher;
        readonly INameNormaliser nameNormaliser = nameNormaliser;

        protected override void LoadData()
        {
            ConcurrentDictionary<string, IDictionary<string, Localisation>> concurrentLocalisations = new();

            Parallel.ForEach(locationGameIds, locationGameId =>
            {
                IDictionary<string, Localisation> locationLocalisations = localisationFetcher
                    .GetGameLocationLocalisations(locationGameId.Id, Settings.Mod.Game)
                    .ToDictionary(x => x.LanguageGameId, x => x);

                concurrentLocalisations.TryAdd(locationGameId.Id, locationLocalisations);
            });

            localisations = concurrentLocalisations.ToDictionary(x => x.Key, x => x.Value);
            locationGameIdsById = locationGameIds
                .GroupBy(gameId => gameId.Id)
                .ToDictionary(group => group.Key, group => group.First());
        }

        protected override void GenerateFiles()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
            string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "localization");
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
            string provinceNamesDirectoryPath = Path.Combine(commonDirectoryPath, "province_names");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(commonDirectoryPath);
            Directory.CreateDirectory(localisationDirectoryPath);
            Directory.CreateDirectory(provinceNamesDirectoryPath);

            CreateDataFiles(provinceNamesDirectoryPath);
            CreateLocalisationFiles(localisationDirectoryPath);
            CreateDescriptorFiles();
        }

        void CreateDataFiles(string provinceNamesDirectoryPath)
        {
            Parallel.ForEach(languageGameIds, languageGameId =>
            {
                string path = Path.Combine(provinceNamesDirectoryPath, $"{languageGameId.Id.ToLower()}.txt");
                StringBuilder contentBuilder = new();
                contentBuilder.Append($"{languageGameId.Id} = {{");
                contentBuilder.Append(Environment.NewLine);

                foreach (string provinceId in localisations.Keys.OrderBy(provinceId => int.Parse(provinceId)))
                {
                    if (!localisations[provinceId].ContainsKey(languageGameId.Id))
                    {
                        continue;
                    }

                    Localisation localisation = localisations[provinceId][languageGameId.Id];

                    contentBuilder.Append(
                        $"    {localisation.GameId} = PROV{localisation.GameId}_{languageGameId.Id} # {nameNormaliser.ToImperatorRomeCharset(localisation.Name)}");

                    if (Settings.Output.AreVerboseCommentsEnabled)
                    {
                        contentBuilder.Append($" # Language={localisation.LanguageId}");
                    }

                    if (!string.IsNullOrWhiteSpace(localisation.Comment))
                    {
                        contentBuilder.Append($" # {localisation.Comment}");
                    }

                    contentBuilder.Append(Environment.NewLine);
                }

                contentBuilder.Append("}");

                File.WriteAllText(path, contentBuilder.ToString());
            });
        }

        void CreateLocalisationFiles(string localisationDirectoryPath)
        {
            string content = GenerateLocalisationFileContent();

            Parallel.ForEach(
                ["english", "french", "german", "spanish"],
                fileLanguage => CreateLocalisationFile(localisationDirectoryPath, fileLanguage, content));
        }

        void CreateLocalisationFile(string localisationDirectoryPath, string language, string content)
        {
            string fileContent = $"l_{language}:{Environment.NewLine}{content}";
            string fileName = $"{Settings.Mod.Id}_provincenames_l_{language}.yml";
            string filePath = Path.Combine(localisationDirectoryPath, fileName);

            File.WriteAllText(filePath, fileContent, Encoding.UTF8);
        }

        void CreateDescriptorFiles()
        {
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateInnerDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{Settings.Mod.Id}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id, $"descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }

        string GenerateLocalisationFileContent()
        {
            List<string> lines = [];
            object lineCollectionLock = new();

            Parallel.ForEach(
                localisations,
                () => new List<string>(),
                (provinceLocalisationsEntry, _, localLines) =>
                {
                    string provinceId = provinceLocalisationsEntry.Key;
                    IDictionary<string, Localisation> provinceLocalisations = provinceLocalisationsEntry.Value;
                    GameId gameId = locationGameIdsById[provinceId];

                    Localisation defaultLocalisation = provinceLocalisations.Values
                        .FirstOrDefault(localisation => localisation.LanguageId.Equals(gameId.DefaultNameLanguageId));

                    if (defaultLocalisation is not null)
                    {
                        localLines.Add(GenerateLocationLocalisationLine(
                            defaultLocalisation,
                            $"PROV{provinceId}"));
                    }

                    foreach (string culture in provinceLocalisations.Keys.OrderBy(culture => culture))
                    {
                        Localisation localisation = provinceLocalisations[culture];

                        localLines.Add(GenerateLocationLocalisationLine(
                            localisation,
                            $"PROV{provinceId}_{localisation.LanguageGameId}"));
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

        string GenerateLocationLocalisationLine(Localisation localisation, string localisationKey)
        {
            string provinceLocalisationDefinition =
                $" {localisationKey}:0 " +
                $"\"{nameNormaliser.ToImperatorRomeCharset(localisation.Name)}\"";

            if (Settings.Output.AreVerboseCommentsEnabled)
            {
                provinceLocalisationDefinition += $" # Language={localisation.LanguageId}";
            }

            if (!string.IsNullOrWhiteSpace(localisation.Comment))
            {
                provinceLocalisationDefinition += $" # {localisation.Comment}";
            }

            return provinceLocalisationDefinition;
        }

        string GenerateMainDescriptorContent()
            => GenerateInnerDescriptorContent() +
                Environment.NewLine +
                $"path=\"mod/{Settings.Mod.Id}\"";

        string GenerateInnerDescriptorContent()
            =>  $"# Version {Settings.Mod.Version} ({DateTime.Now})" + Environment.NewLine +
                $"name=\"{Settings.Mod.Name}\"" + Environment.NewLine +
                $"version=\"{Settings.Mod.Version}\"" + Environment.NewLine +
                $"supported_version=\"{Settings.Mod.GameVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"}}";
    }
}
