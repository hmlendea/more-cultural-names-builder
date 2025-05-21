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
    public sealed class HOI4ModBuilder : ModBuilder
    {
        IEnumerable<GameId> stateGameIds;
        IEnumerable<GameId> cityGameIds;
        IDictionary<string, IDictionary<string, Localisation>> stateLocalisations;
        IDictionary<string, IDictionary<string, Localisation>> cityLocalisations;

        readonly ILocalisationFetcher localisationFetcher;
        readonly INameNormaliser nameNormaliser;

        public HOI4ModBuilder(
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
            stateGameIds = locations.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Settings.Mod.Game && x.Type.Equals("State"))
                .OrderBy(x => int.Parse(x.Id));

            cityGameIds = locations.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Settings.Mod.Game && x.Type.Equals("City"))
                .OrderBy(x => int.Parse(x.Id));

            stateLocalisations = new ConcurrentDictionary<string, IDictionary<string, Localisation>>();
            cityLocalisations = new ConcurrentDictionary<string, IDictionary<string, Localisation>>();

            Parallel.ForEach(stateGameIds, stateGameId =>
            {
                IDictionary<string, Localisation> localisations = localisationFetcher
                    .GetGameLocationLocalisations(stateGameId.Id, "State", Settings.Mod.Game)
                    .ToDictionary(x => x.LanguageGameId, x => x);

                stateLocalisations.Add(stateGameId.Id, localisations);
            });

            Parallel.ForEach(cityGameIds, cityGameId =>
            {
                IDictionary<string, Localisation> localisations = localisationFetcher
                    .GetGameLocationLocalisations(cityGameId.Id, "City", Settings.Mod.Game)
                    .ToDictionary(x => x.LanguageGameId, x => x);

                cityLocalisations.Add(cityGameId.Id, localisations);
            });
        }

        protected override void GenerateFiles()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
            string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "localisation");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(localisationDirectoryPath);

            LoadData();

            CreateLocalisationFiles(localisationDirectoryPath);
            CreateDescriptorFiles();
        }

        void CreateLocalisationFiles(string localisationDirectoryPath)
        {
            string content = GenerateCityLocalisationFileContent();

            Parallel.ForEach(["english", "french", "german", "polish", "spanish"], locale =>
            {
                string fileDirectoryPath = Path.Combine(localisationDirectoryPath, locale);
                string fileContent = $"l_{locale}:{Environment.NewLine}{content}";
                string fileName = $"zzz999_{Settings.Mod.Id}_l_{locale}.yml";
                string filePath = Path.Combine(fileDirectoryPath, fileName);

                Directory.CreateDirectory(fileDirectoryPath);
                File.WriteAllText(filePath, fileContent, Encoding.UTF8);
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

        string GenerateCityLocalisationFileContent()
        {
            ConcurrentBag<string> lines = [];

            // States
            Parallel.ForEach(stateLocalisations.Keys, stateId =>
            {
                foreach (Localisation localisation in stateLocalisations[stateId].Values)
                {
                    lines.Add(
                        $" {localisation.LanguageGameId}_STATE_{stateId}:0 " +
                        $"\"{nameNormaliser.ToHOI4StateCharset(localisation.Name)}\"");
                }
            });

            // Cities
            Parallel.ForEach(cityLocalisations.Keys, cityId =>
            {
                foreach (Localisation localisation in cityLocalisations[cityId].Values)
                {
                    lines.Add(
                        $" {localisation.LanguageGameId}_VICTORY_POINTS_{cityId}:0 " +
                        $"\"{nameNormaliser.ToHOI4CityCharset(localisation.Name)}\"");
                }
            });

            return string.Join(
                Environment.NewLine,
                lines.OrderBy(line => line));
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
