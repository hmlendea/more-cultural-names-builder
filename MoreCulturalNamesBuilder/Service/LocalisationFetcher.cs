using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Mapping;
using MoreCulturalNamesBuilder.Service.Models;

using NuciDAL.Repositories;

namespace MoreCulturalNamesBuilder.Service
{
    public sealed class LocalisationFetcher : ILocalisationFetcher
    {
        readonly IFileRepository<LanguageEntity> languageRepository;
        readonly IFileRepository<LocationEntity> locationRepository;

        readonly ConcurrentDictionary<string, Dictionary<string, string>> languageGameIdsCache;

        Dictionary<string, Location> locations;
        Dictionary<string, Language> languages;

        readonly Dictionary<(string Game, string Id), Location> locationGameIdIndex;
        readonly Dictionary<(string Game, string GameLanguageId), string> gameLanguageIdToLanguageId;

        public LocalisationFetcher(
            IFileRepository<LanguageEntity> languageRepository,
            IFileRepository<LocationEntity> locationRepository)
        {
            this.languageRepository = languageRepository;
            this.locationRepository = locationRepository;

            languageGameIdsCache = [];
            locationGameIdIndex = [];
            gameLanguageIdToLanguageId = [];

            LoadData();
        }

        void LoadData()
        {
            locations = locationRepository
                .GetAll()
                .ToServiceModels()
                .ToDictionary(location => location.Id, location => location);

            languages = languageRepository
                .GetAll()
                .ToServiceModels()
                .ToDictionary(language => language.Id, language => language);

            locationGameIdIndex.Clear();
            gameLanguageIdToLanguageId.Clear();

            foreach (Location loc in locations.Values)
            {
                foreach (GameId gameId in loc.GameIds)
                {
                    locationGameIdIndex[(gameId.Game, gameId.Id)] = loc;
                }
            }

            foreach (var language in languages.Values)
            {
                foreach (var gameId in language.GameIds)
                {
                    gameLanguageIdToLanguageId[(gameId.Game, gameId.Id)] = language.Id;
                }
            }
        }

        public IEnumerable<Localisation> GetGameLocationLocalisations(
            string locationGameId,
            string game)
            => GetGameLocationLocalisations(locationGameId, null, game);

        public IEnumerable<Localisation> GetGameLocationLocalisations(
            string locationGameId,
            string locationGameIdType,
            string game)
        {
            List<Localisation> localisations = [];
            Location location;

            if (string.IsNullOrWhiteSpace(locationGameIdType))
            {
                locationGameIdIndex.TryGetValue((game, locationGameId), out location);
            }
            else
            {
                location = locations.Values.FirstOrDefault(x => x.GameIds.Any(x =>
                    x.Game == game &&
                    x.Id == locationGameId &&
                    x.Type == locationGameIdType));
            }

            if (location is null)
            {
                return localisations;
            }

            foreach (var languageGameId in GetLanguageGameIds(game))
            {
                Localisation localisation = GetLocationLocalisation(location, languageGameId.Value);

                if (localisation is null)
                {
                    continue;
                }

                localisation.GameId = locationGameId;
                localisation.LanguageGameId = languageGameId.Key;

                localisations.Add(localisation);
            };

            return localisations;
        }

        Localisation GetLocationLocalisation(Location location, string languageId)
        {
            if (location.IsEmpty())
            {
                return null;
            }

            Language language = languages[languageId];

            List<string> locationIdsToCheck = [location.Id];
            List<string> languageIdsToCheck = [language.Id];

            locationIdsToCheck.AddRange(location.FallbackLocations);
            languageIdsToCheck.AddRange(language.FallbackLanguages);

            foreach (string locationIdToCheck in locationIdsToCheck)
            {
                Location locationToCheck = locations[locationIdToCheck];

                foreach (string languageIdToCheck in languageIdsToCheck)
                {
                    Name name = locationToCheck.Names.FirstOrDefault(name => name.LanguageId.Equals(languageIdToCheck));

                    if (name is not null)
                    {
                        return new()
                        {
                            Id = locationIdToCheck,
                            LanguageId = languageIdToCheck,
                            Name = name.Value,
                            Adjective = name.Adjective,
                            Comment = name.Comment
                        };
                    }
                }
            }

            return null;
        }

        Dictionary<string, string> GetLanguageGameIds(string game)
        {
            if (languageGameIdsCache.TryGetValue(game, out Dictionary<string, string> value))
            {
                return value;
            }

            Dictionary<string, string> languageGameIds = gameLanguageIdToLanguageId
                .Where(kvp => kvp.Key.Game.Equals(game))
                .ToDictionary(
                    kvp => kvp.Key.GameLanguageId,
                    kvp => kvp.Value);

            languageGameIdsCache.TryAdd(game, languageGameIds);

            return languageGameIds;
        }
    }
}
