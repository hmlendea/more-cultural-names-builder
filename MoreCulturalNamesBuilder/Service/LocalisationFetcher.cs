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

        static Dictionary<string, string> EmptyLanguageGameIds => [];

        Dictionary<string, Location> locations;
        Dictionary<string, Language> languages;

        readonly Dictionary<(string Game, string Id), Location> locationGameIdIndex;
        readonly Dictionary<(string Game, string Id, string Type), Location> locationGameIdWithTypeIndex;
        readonly Dictionary<string, Dictionary<string, string>> languageGameIdsByGame;
        readonly Dictionary<string, Dictionary<string, Name>> locationNamesByLanguage;
        readonly Dictionary<string, string[]> locationIdsToCheckByLocationId;
        readonly Dictionary<string, string[]> languageIdsToCheckByLanguageId;

        public LocalisationFetcher(
            IFileRepository<LanguageEntity> languageRepository,
            IFileRepository<LocationEntity> locationRepository)
        {
            this.languageRepository = languageRepository;
            this.locationRepository = locationRepository;

            locationGameIdIndex = [];
            locationGameIdWithTypeIndex = [];
            languageGameIdsByGame = [];
            locationNamesByLanguage = [];
            locationIdsToCheckByLocationId = [];
            languageIdsToCheckByLanguageId = [];

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
            locationGameIdWithTypeIndex.Clear();
            languageGameIdsByGame.Clear();
            locationNamesByLanguage.Clear();
            locationIdsToCheckByLocationId.Clear();
            languageIdsToCheckByLanguageId.Clear();

            foreach (Location location in locations.Values)
            {
                locationNamesByLanguage[location.Id] = BuildNameIndex(location);
                locationIdsToCheckByLocationId[location.Id] = BuildLocationIdsToCheck(location);

                foreach (GameId gameId in location.GameIds)
                {
                    locationGameIdIndex[(gameId.Game, gameId.Id)] = location;
                    locationGameIdWithTypeIndex[(gameId.Game, gameId.Id, gameId.Type)] = location;
                }
            }

            foreach (Language language in languages.Values)
            {
                languageIdsToCheckByLanguageId[language.Id] = BuildLanguageIdsToCheck(language);

                foreach (GameId gameId in language.GameIds)
                {
                    if (!languageGameIdsByGame.TryGetValue(gameId.Game, out Dictionary<string, string> gameLanguageIds))
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
                locationGameIdWithTypeIndex.TryGetValue((game, locationGameId, locationGameIdType), out location);
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

            string[] locationIdsToCheck = locationIdsToCheckByLocationId[location.Id];
            string[] languageIdsToCheck = languageIdsToCheckByLanguageId[languageId];

            foreach (string locationIdToCheck in locationIdsToCheck)
            {
                Dictionary<string, Name> namesByLanguage = locationNamesByLanguage[locationIdToCheck];

                foreach (string languageIdToCheck in languageIdsToCheck)
                {
                    if (!namesByLanguage.TryGetValue(languageIdToCheck, out Name name))
                    {
                        continue;
                    }

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
            if (languageGameIdsByGame.TryGetValue(game, out Dictionary<string, string> languageGameIds))
            {
                return languageGameIds;
            }

            return EmptyLanguageGameIds;
        }

        static Dictionary<string, Name> BuildNameIndex(Location location)
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

        static string[] BuildLocationIdsToCheck(Location location)
        {
            List<string> locationIdsToCheck = [location.Id];
            locationIdsToCheck.AddRange(location.FallbackLocations);

            return [.. locationIdsToCheck];
        }

        static string[] BuildLanguageIdsToCheck(Language language)
        {
            List<string> languageIdsToCheck = [language.Id];
            languageIdsToCheck.AddRange(language.FallbackLanguages);

            return [.. languageIdsToCheck];
        }
    }
}
