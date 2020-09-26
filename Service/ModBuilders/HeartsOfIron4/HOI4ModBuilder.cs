using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.HeartsOfIron4
{
    public sealed class HOI4ModBuilder : ModBuilder, IHOI4ModBuilder
    {
        const string CityEventsFileName = "MoreCulturalNames_Cities.txt";
        const string StateEventsFileName = "MoreCulturalNames_States.txt";

        public override string Game => "HOI4";

        readonly ILocalisationFetcher localisationFetcher;
        readonly INameNormaliser nameNormaliser;

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

        protected override void BuildMod()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, outputSettings.HOI4ModId);
            string eventsDirectoryPath = Path.Combine(mainDirectoryPath, "events");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(eventsDirectoryPath);

            CreateDescriptorFiles();
            CreateStateEventsFile(eventsDirectoryPath);
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

        void CreateStateEventsFile(string eventsDirectoryPath)
        {
            string stateEventsFilePath = Path.Combine(eventsDirectoryPath, StateEventsFileName);

            IEnumerable<GameId> gameLocationIds = locations.Values
                    .SelectMany(x => x.GameIds)
                    .Where(x => x.Game == Game)
                    .OrderBy(x => x.Id);

            IList<string> eventContents = new List<string>();
            
            foreach (GameId gameLocationId in gameLocationIds.Where(x => x.Type == "State"))
            {
                string locationEvents = GenerateStateEvents(gameLocationId);

                eventContents.Add(locationEvents);
            }

            string eventsContent = string.Join(Environment.NewLine, eventContents);
            File.WriteAllText(stateEventsFilePath, eventsContent);
        }

        string GenerateStateEvents(GameId gameId)
        {
            IEnumerable<Localisation> localisations = localisationFetcher.GetGameLocationLocalisations(gameId.Id, Game);

            string entireContent = $"##### MCN ##### State={gameId.Id}" + Environment.NewLine;

            foreach (Localisation localisation in localisations.OrderBy(x => x.LanguageGameId))
            {
                string eventId = $"mcn_{localisation.LanguageGameId}.{gameId.Id}";
                string name = nameNormaliser.ToHOI4(localisation.Name);

                string eventContent =
                    $"# State={gameId.Id}, Country={localisation.LanguageGameId}, InGameName=\"{name}\", RealName=\"{localisation.Name}\", Event={eventId}" + Environment.NewLine +
                    $"country_event = {{" + Environment.NewLine +
                    $"    id = {eventId}" + Environment.NewLine +
                    $"    title = {eventId}.title" + Environment.NewLine +
                    $"    desc = {eventId}.description" + Environment.NewLine +
                    $"    picture = GFX_report_event_german_reichstag_gathering" + Environment.NewLine +
                    $"    hidden = yes" + Environment.NewLine +
                    $"    trigger = {{ {localisation.LanguageGameId} = {{ owns_state = {gameId.Id} }} }}" + Environment.NewLine +
                    $"    mean_time_to_happen = {{ days = 3 }}" + Environment.NewLine +
                    $"    immediate = {{ hidden_effect = {{ {gameId.Id} = {{ set_state_name = \"{name}\" }} }} }}" + Environment.NewLine +
                    $"    option = {{ name = {eventId}.option }}" + Environment.NewLine +
                    $"}}" + Environment.NewLine;

                entireContent += eventContent + Environment.NewLine;
            }

            return entireContent;
        }
        
        string GenerateMainDescriptorContent()
        {
            return GenerateInnerDescriptorContent() + Environment.NewLine +
                $"path=\"mod/{outputSettings.HOI4ModId}\"";
        }

        string GenerateInnerDescriptorContent()
        {
            return
                $"name=\"{outputSettings.HOI4ModName}\"" + Environment.NewLine +
                $"version=\"{outputSettings.ModVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"    \"Map\"" + Environment.NewLine +
                $"    \"Translation\"" + Environment.NewLine +
                $"}}" + Environment.NewLine +
                $"supported_version=\"{outputSettings.HOI4GameVersion}\"";
        }
    }
}
