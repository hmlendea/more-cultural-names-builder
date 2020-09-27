using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuciDAL.Repositories;
using NuciExtensions;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.HeartsOfIron4
{
    public sealed class HOI4ModBuilder : ModBuilder, IHOI4ModBuilder
    {
        const string EventsFileName = "873_MoreCulturalNames.txt";

        public override string Game => "HOI4";

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
            OutputSettings outputSettings)
            : base(
                languageRepository,
                locationRepository,
                outputSettings)
        {
            this.localisationFetcher = localisationFetcher;
            this.nameNormaliser = nameNormaliser;
        }

        protected override void LoadData()
        {
            countryTags = languages.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Game)
                .Select(x => x.Id);

            stateGameIds = locations.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Game && x.Type == "State")
                .OrderBy(x => int.Parse(x.Id));

            cityGameIds = locations.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Game && x.Type == "City")
                .OrderBy(x => int.Parse(x.Id));

            stateCities = cityGameIds
                .GroupBy(x => x.Parent)
                .ToDictionary(x => x.Key, x => x.ToList());

            stateLocalisations = new Dictionary<string, IDictionary<string, Localisation>>();
            cityLocalisations = new Dictionary<string, IDictionary<string, Localisation>>();

            foreach (GameId stateGameId in stateGameIds)
            {
                IDictionary<string, Localisation> localisations = localisationFetcher
                    .GetGameLocationLocalisations(stateGameId.Id, Game)
                    .ToDictionary(x => x.LanguageGameId, x => x);

                stateLocalisations.Add(stateGameId.Id, localisations);
            }

            foreach (GameId cityGameId in cityGameIds)
            {
                IDictionary<string, Localisation> localisations = localisationFetcher
                    .GetGameLocationLocalisations(cityGameId.Id, Game)
                    .ToDictionary(x => x.LanguageGameId, x => x);

                cityLocalisations.Add(cityGameId.Id, localisations);
            }
        }

        protected override void GenerateFiles()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, outputSettings.HOI4ModId);
            string eventsDirectoryPath = Path.Combine(mainDirectoryPath, "events");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(eventsDirectoryPath);

            LoadData();

            CreateDescriptorFiles();
            CreateEventsFile(eventsDirectoryPath);
        }

        void CreateDescriptorFiles()
        {
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateInnerDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{outputSettings.HOI4ModId}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, outputSettings.HOI4ModId, $"descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }

        void CreateEventsFile(string eventsDirectoryPath)
        {
            string eventsFilePath = Path.Combine(eventsDirectoryPath, EventsFileName);

            IList<string> eventContents = new List<string>();
            
            foreach (GameId gameLocationId in stateGameIds.OrderBy(x => int.Parse(x.Id)))
            {
                string locationEvents = GenerateStateEvents(gameLocationId);

                eventContents.Add(locationEvents);
            }

            string eventsContent = string.Join(Environment.NewLine, eventContents);
            File.WriteAllText(eventsFilePath, eventsContent);
        }

        string GenerateStateEvents(GameId stateGameId)
        {
            string entireContent =
                $"############### MCN ############### State={stateGameId.Id} ###############" +
                Environment.NewLine + Environment.NewLine + Environment.NewLine;

            foreach (string countryTag in countryTags)
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

            string eventContent = $"# Event={eventId}, State={stateGameId.Id}, Country={countryTag}";
            string nameSetsEventContent = string.Empty;
            
            Localisation stateLocalisation = stateLocalisations
                .TryGetValue(stateGameId.Id)
                .TryGetValue(countryTag);

            if (!(stateLocalisation is null))
            {
                stateName = nameNormaliser.ToHOI4Charset(stateLocalisation.Name);
                
                eventContent += $", LocalisedName=\"{stateName}\"";
                nameSetsEventContent +=
                    $"            {stateGameId.Id} = {{ set_state_name = \"{stateName}\" }}" + 
                    $" # Name={stateLocalisation.Name}, Language={stateLocalisation.LanguageId}" + Environment.NewLine;

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

                    string cityName = nameNormaliser.ToHOI4Charset(cityLocalisation.Name);
                    
                    nameSetsEventContent +=
                        $"            set_province_name = {{ id = {cityGameId.Id} name = \"{cityName}\" }}" +
                        $" # Name={cityLocalisation.Name}, Language={cityLocalisation.LanguageId}" + Environment.NewLine;
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
        {
            return GenerateInnerDescriptorContent() + Environment.NewLine +
                $"path=\"mod/{outputSettings.HOI4ModId}\"";
        }

        string GenerateInnerDescriptorContent()
        {
            return
                $"# Version {outputSettings.ModVersion} ({DateTime.Now})" + Environment.NewLine +
                $"name=\"{outputSettings.HOI4ModName}\"" + Environment.NewLine +
                $"version=\"{outputSettings.ModVersion}\"" + Environment.NewLine +
                $"supported_version=\"{outputSettings.HOI4GameVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"    \"Map\"" + Environment.NewLine +
                $"    \"Translation\"" + Environment.NewLine +
                $"}}";
        }
    }
}
