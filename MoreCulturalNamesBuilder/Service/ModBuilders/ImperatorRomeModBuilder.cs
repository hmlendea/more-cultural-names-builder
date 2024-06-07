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
    public sealed class ImperatorRomeModBuilder : ModBuilder
    {
        IDictionary<string, IDictionary<string, Localisation>> localisations;

        readonly ILocalisationFetcher localisationFetcher;
        readonly INameNormaliser nameNormaliser;

        public ImperatorRomeModBuilder(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            Settings settings)
            : base(languageRepository, locationRepository, settings)
        {
            this.localisationFetcher = localisationFetcher;
            this.nameNormaliser = nameNormaliser;
        }

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

            LoadData();
            CreateDataFiles(provinceNamesDirectoryPath);
            CreateLocalisationFiles(localisationDirectoryPath);
            CreateDescriptorFiles();
        }

        void CreateDataFiles(string provinceNamesDirectoryPath)
        {
            Parallel.ForEach(languageGameIds, languageGameId =>
            {
                string path = Path.Combine(provinceNamesDirectoryPath, $"{languageGameId.Id.ToLower()}.txt");
                string content = $"{languageGameId.Id} = {{" + Environment.NewLine;

                foreach (string provinceId in localisations.Keys.OrderBy(x => int.Parse(x)))
                {
                    if (!localisations[provinceId].ContainsKey(languageGameId.Id))
                    {
                        continue;
                    }

                    Localisation localisation = localisations[provinceId][languageGameId.Id];

                    content += $"    {localisation.GameId} = PROV{localisation.GameId}_{languageGameId.Id} # {nameNormaliser.ToImperatorRomeCharset(localisation.Name)}";

                    if (Settings.Output.AreVerboseCommentsEnabled)
                    {
                        content += $" # Language={localisation.LanguageId}";
                    }

                    if (!string.IsNullOrWhiteSpace(localisation.Comment))
                    {
                        content += $" # {localisation.Comment}";
                    }

                    content += Environment.NewLine;
                }

                content += "}";

                File.WriteAllText(path, content);
            });
        }

        void CreateLocalisationFiles(string localisationDirectoryPath)
        {
            string content = GenerateLocalisationFileContent();

            Parallel.ForEach(
                new List<string>{ "english", "french", "german", "spanish" },
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
            ConcurrentBag<string> lines = [];

            Parallel.ForEach(localisations.Keys, provinceId =>
            {
                GameId gameId = locationGameIds.First(x => x.Id.Equals(provinceId));

                IDictionary<string, Localisation> provinceLocalisations = localisations[provinceId];
                Localisation defaultLocalisation = provinceLocalisations.Values
                    .FirstOrDefault(x => x.LanguageId.Equals(gameId.DefaultNameLanguageId));

                if (defaultLocalisation is not null)
                {
                    string provinceDefaultLocalisationDefinition = GenerateLocationLocalisationLine(
                        defaultLocalisation,
                        $"PROV{provinceId}");

                    lines.Add(provinceDefaultLocalisationDefinition);
                }

                foreach (string culture in provinceLocalisations.Keys.OrderBy(x => x))
                {
                    Localisation localisation = provinceLocalisations[culture];

                    string provinceCulturalLocalisationDefinition = GenerateLocationLocalisationLine(
                        localisation,
                        $"PROV{provinceId}_{localisation.LanguageGameId}");

                    lines.Add(provinceCulturalLocalisationDefinition);
                }
            });

            return string.Join(
                Environment.NewLine,
                lines.OrderBy(line => line));
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
