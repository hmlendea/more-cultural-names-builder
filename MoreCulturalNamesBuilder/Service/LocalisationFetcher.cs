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
        private readonly IFileRepository<LanguageDataObject> languageRepository;
        private readonly IFileRepository<LocationDataObject> locationRepository;
        private readonly Dictionary<GameLocationKey, Location> locationGameIdIndex;
        private readonly Dictionary<TypedGameLocationKey, Location> locationGameIdWithTypeIndex;
        private readonly Dictionary<string, Dictionary<string, string>> languageGameIdsByGame;
        private readonly Dictionary<string, Dictionary<string, Name>> locationNamesByLanguage;
        private readonly Dictionary<string, string[]> locationIdsToCheckByLocationId;
        private readonly Dictionary<string, string[]> languageIdsToCheckByLanguageId;
        private readonly ConcurrentDictionary<LocalisationKey, CachedLocalisation> resolvedLocalisationCache;

        private Dictionary<string, Location> locations = null!;
        private Dictionary<string, Language> languages = null!;

        public LocalisationFetcher(
            IFileRepository<LanguageDataObject> languageRepository,
            IFileRepository<LocationDataObject> locationRepository)
        {
            this.languageRepository = languageRepository;
            this.locationRepository = locationRepository;

            locationGameIdIndex = [];
            locationGameIdWithTypeIndex = [];
            languageGameIdsByGame = [];
            locationNamesByLanguage = [];
            locationIdsToCheckByLocationId = [];
            languageIdsToCheckByLanguageId = [];
            resolvedLocalisationCache = [];

            LoadData();
        }

        private void LoadData()
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
            locationGameIdWithTypeIndex.Clear();
            languageGameIdsByGame.Clear();
            locationNamesByLanguage.Clear();
            locationIdsToCheckByLocationId.Clear();
            languageIdsToCheckByLanguageId.Clear();
            resolvedLocalisationCache.Clear();

            foreach (Location location in locations.Values)
            {
                locationNamesByLanguage[location.Id] = BuildNameIndex(location);
                locationIdsToCheckByLocationId[location.Id] = BuildLocationIdsToCheck(location);

                foreach (GameId gameId in location.GameIds)
                {
                    locationGameIdIndex[new(gameId.Game, gameId.Id)] = location;
                    locationGameIdWithTypeIndex[new(gameId.Game, gameId.Id, gameId.Type)] = location;
                }
            }

            foreach (Language language in languages.Values)
            {
                languageIdsToCheckByLanguageId[language.Id] = BuildLanguageIdsToCheck(language);

                foreach (GameId gameId in language.GameIds)
                {
                    if (!languageGameIdsByGame.TryGetValue(gameId.Game, out Dictionary<string, string>? gameLanguageIds))
                    {
                        gameLanguageIds = [];
                        languageGameIdsByGame[gameId.Game] = gameLanguageIds;
                    }

                    gameLanguageIds[gameId.Id] = language.Id;
                }
            }
        }

        public IEnumerable<Localisation> GetGameLocationLocalisations(
            string locationGameId,
            string game)
            => GetGameLocationLocalisations(locationGameId, null, game);

        public IEnumerable<Localisation> GetGameLocationLocalisations(
            string locationGameId,
            string? locationGameIdType,
            string game)
        {
            List<Localisation> localisations = [];
            Location? location;

            if (string.IsNullOrWhiteSpace(locationGameIdType))
            {
                locationGameIdIndex.TryGetValue(new(game, locationGameId), out location);
            }
            else
            {
                locationGameIdWithTypeIndex.TryGetValue(new(game, locationGameId, locationGameIdType), out location);
            }

            if (location is null)
            {
                return localisations;
            }

            foreach (KeyValuePair<string, string> languageGameId in GetLanguageGameIds(game))
            {
                Localisation? localisation = GetLocationLocalisation(location, languageGameId.Value);

                if (localisation is null)
                {
                    continue;
                }

                localisation.GameId = locationGameId;
                localisation.LanguageGameId = languageGameId.Key;

                localisations.Add(localisation);
            }

            return localisations;
        }

        private Localisation? GetLocationLocalisation(Location location, string languageId)
        {
            if (location.IsEmpty())
            {
                return null;
            }

            LocalisationKey localisationCacheKey = new(location.Id, languageId);

            if (resolvedLocalisationCache.TryGetValue(localisationCacheKey, out CachedLocalisation? cachedLocalisation))
            {
                return cachedLocalisation.ToServiceModel();
            }

            string[] locationIdsToCheck = locationIdsToCheckByLocationId[location.Id];
            string[] languageIdsToCheck = languageIdsToCheckByLanguageId[languageId];

            foreach (string locationIdToCheck in locationIdsToCheck)
            {
                Dictionary<string, Name> namesByLanguage = locationNamesByLanguage[locationIdToCheck];

                foreach (string languageIdToCheck in languageIdsToCheck)
                {
                    if (!namesByLanguage.TryGetValue(languageIdToCheck, out Name? name))
                    {
                        continue;
                    }

                    if (name is not null)
                    {
                        CachedLocalisation resolvedLocalisation = new(
                            true,
                            locationIdToCheck,
                            languageIdToCheck,
                            name.Value,
                            name.Adjective,
                            name.Comment);

                        resolvedLocalisationCache.TryAdd(localisationCacheKey, resolvedLocalisation);

                        return resolvedLocalisation.ToServiceModel();
                    }
                }
            }

            CachedLocalisation missingLocalisation = new(
                false,
                null,
                null,
                null,
                null,
                null);

            resolvedLocalisationCache.TryAdd(localisationCacheKey, missingLocalisation);

            return null;
        }

        private IEnumerable<KeyValuePair<string, string>> GetLanguageGameIds(string game)
        {
            if (languageGameIdsByGame.TryGetValue(game, out Dictionary<string, string>? languageGameIds))
            {
                return languageGameIds;
            }

            return [];
        }

        private static Dictionary<string, Name> BuildNameIndex(Location location)
        {
            Dictionary<string, Name> namesByLanguage = [];

            foreach (Name name in location.Names)
            {
                if (!namesByLanguage.ContainsKey(name.LanguageId))
                {
                    namesByLanguage[name.LanguageId] = name;
                }
            }

            return namesByLanguage;
        }

        private static string[] BuildLocationIdsToCheck(Location location)
        {
            List<string> locationIdsToCheck = [location.Id];
            locationIdsToCheck.AddRange(location.FallbackLocations);

            return [.. locationIdsToCheck];
        }

        private static string[] BuildLanguageIdsToCheck(Language language)
        {
            List<string> languageIdsToCheck = [language.Id];
            languageIdsToCheck.AddRange(language.FallbackLanguages);

            return [.. languageIdsToCheck];
        }

    }
}
