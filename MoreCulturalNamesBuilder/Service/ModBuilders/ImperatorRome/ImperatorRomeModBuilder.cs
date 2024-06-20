using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NuciDAL.Repositories;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.ModBuilders.ImperatorRome
{
    public sealed class ImperatorRomeModBuilder(
        ILocalisationFetcher localisationFetcher,
        INameNormaliser nameNormaliser,
        IRepository<LanguageEntity> languageRepository,
        IRepository<LocationEntity> locationRepository,
        IImperatorRomeLocalisationsBuilder localisationsBuilder,
        Settings settings) : ModBuilder(languageRepository, locationRepository, settings)
    {
        IDictionary<string, IDictionary<string, Localisation>> localisations;

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
            localisationsBuilder.CreateLocalisationFiles(localisationDirectoryPath, localisations, locationGameIds);
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

        void CreateDescriptorFiles()
        {
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateInnerDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{Settings.Mod.Id}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id, $"descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
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
