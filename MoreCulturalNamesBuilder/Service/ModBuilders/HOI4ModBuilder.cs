using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NuciDAL.Repositories;
using NuciExtensions;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class HOI4ModBuilder : ModBuilder
    {
        const string EventsFileNameFormat = "873_mcn_{0}.txt";

        readonly ILocalisationFetcher localisationFetcher;
        readonly INameNormaliser nameNormaliser;

        IEnumerable<string> countryTags;
        IEnumerable<GameId> stateGameIds;
        IEnumerable<GameId> cityGameIds;
        IDictionary<string, List<GameId>> stateCities;
        IDictionary<string, IDictionary<string, Localisation>> stateLocalisations;
        IDictionary<string, IDictionary<string, Localisation>> cityLocalisations;

        public HOI4ModBuilder(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            IRepository<TitleEntity> titleRepository,
            Settings settings)
            : base(
                languageRepository,
                locationRepository,
                titleRepository,
                settings)
        {
            this.localisationFetcher = localisationFetcher;
            this.nameNormaliser = nameNormaliser;
        }

        protected override void LoadData()
        {
            countryTags = languages.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Settings.Mod.Game)
                .Select(x => x.Id);

            stateGameIds = locations.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Settings.Mod.Game && x.Type == "State")
                .OrderBy(x => int.Parse(x.Id));

            cityGameIds = locations.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Settings.Mod.Game && x.Type == "City")
                .OrderBy(x => int.Parse(x.Id));

            stateCities = cityGameIds
                .GroupBy(x => x.Parent)
                .ToDictionary(x => x.Key, x => x.ToList());

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
            string eventsDirectoryPath = Path.Combine(mainDirectoryPath, "events");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(eventsDirectoryPath);

            LoadData();

            CreateDescriptorFiles();
            CreateEventsFiles(eventsDirectoryPath);
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

        void CreateEventsFiles(string eventsDirectoryPath)
        {
            Parallel.ForEach(countryTags, countryTag =>
            {
                string eventsFileName = string.Format(EventsFileNameFormat, countryTag);
                string eventsFilePath = Path.Combine(eventsDirectoryPath, eventsFileName);

                string countryEvents = GenerateCountryEvents(countryTag);

                File.WriteAllText(eventsFilePath, countryEvents);
            });
        }

        string GenerateCountryEvents(string countryTag)
        {
            string entireContent =
                $"############### MCN ############### Country={countryTag} ###############" +
                Environment.NewLine + Environment.NewLine + Environment.NewLine;

            foreach (GameId stateGameId in stateGameIds.OrderBy(x => int.Parse(x.Id)))
            {
                string stateEventContentForCountry = GenerateStateEventForCountry(stateGameId, countryTag);

                if (!string.IsNullOrWhiteSpace(stateEventContentForCountry))
                {
                    entireContent += stateEventContentForCountry + Environment.NewLine;
                }
            }

            return entireContent;
        }

        string GenerateStateEventForCountry(GameId stateGameId, string countryTag)
        {
            string eventId = $"mcn_{countryTag}.{stateGameId.Id}";
            string stateName = string.Empty;

            string eventContent = $"# Event={eventId}, State={stateGameId.Id}";
            string nameSetsEventContent = string.Empty;

            Localisation stateLocalisation = stateLocalisations
                .TryGetValue(stateGameId.Id)
                .TryGetValue(countryTag);

            if (!(stateLocalisation is null))
            {
                stateName = nameNormaliser.ToHOI4StateCharset(stateLocalisation.Name);

                eventContent += $", LocalisedName=\"{stateName}\"";
                nameSetsEventContent +=
                    $"            {stateGameId.Id} = {{ set_state_name = \"{stateName}\" }} # {stateLocalisation.Name}";

                if (Settings.Output.AreVerboseCommentsEnabled)
                {
                    nameSetsEventContent += $" # Language={stateLocalisation.LanguageId}";
                }

                if (!string.IsNullOrWhiteSpace(stateLocalisation.Comment))
                {
                    nameSetsEventContent += $" # {stateLocalisation.Comment}";
                }

                nameSetsEventContent += Environment.NewLine;

                if (stateName != stateLocalisation.Name)
                {
                    eventContent += $" # {stateLocalisation.Name}";
                }
            }

            eventContent += Environment.NewLine +
                $"country_event = {{" + Environment.NewLine +
                $"    id = {eventId}" + Environment.NewLine +
                $"    title = {eventId}.title" + Environment.NewLine +
                $"    desc = {eventId}.description" + Environment.NewLine +
                $"    picture = GFX_report_event_german_reichstag_gathering" + Environment.NewLine +
                $"    hidden = yes" + Environment.NewLine +
                $"    trigger = {{ {countryTag} = {{ owns_state = {stateGameId.Id} }} }}" + Environment.NewLine +
                $"    mean_time_to_happen = {{ days = 3 }}" + Environment.NewLine +
                $"    immediate = {{" + Environment.NewLine +
                $"        hidden_effect = {{" + Environment.NewLine;

            IEnumerable<GameId> currentStateCities = stateCities.TryGetValue(stateGameId.Id);

            if (!EnumerableExt.IsNullOrEmpty(currentStateCities))
            {
                foreach (GameId cityGameId in currentStateCities.OrderBy(x => int.Parse(x.Id)))
                {
                    Localisation cityLocalisation = cityLocalisations
                        .TryGetValue(cityGameId.Id)
                        .TryGetValue(countryTag);

                    if (cityLocalisation is null)
                    {
                        continue;
                    }

                    string cityName = nameNormaliser.ToHOI4CityCharset(cityLocalisation.Name);

                    nameSetsEventContent +=
                        $"            set_province_name = {{ id = {cityGameId.Id} name = \"{cityName}\" }} # {cityLocalisation.Name}";

                    if (Settings.Output.AreVerboseCommentsEnabled)
                    {
                        nameSetsEventContent += $" # Language={cityLocalisation.LanguageId}";
                    }

                    if (!string.IsNullOrWhiteSpace(cityLocalisation.Comment))
                    {
                        nameSetsEventContent += $" # {cityLocalisation.Comment}";
                    }

                    nameSetsEventContent += Environment.NewLine;
                }
            }

            if (string.IsNullOrWhiteSpace(nameSetsEventContent))
            {
                return null;
            }

            eventContent += nameSetsEventContent +
                $"        }}" + Environment.NewLine +
                $"    }}" + Environment.NewLine +
                $"    option = {{ name = {eventId}.option }}" + Environment.NewLine +
                $"}}" + Environment.NewLine;

            return eventContent;
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
                $"    \"Map\"" + Environment.NewLine +
                $"    \"Translation\"" + Environment.NewLine +
                $"}}";
    }
}
