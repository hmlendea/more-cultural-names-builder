using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Mapping;
using MoreCulturalNamesModBuilder.Service.Models;

using NuciDAL.Repositories;

namespace MoreCulturalNamesModBuilder.Service
{
    public sealed class LocalisationFetcher : ILocalisationFetcher
    {
        readonly IRepository<LanguageEntity> languageRepository;
        readonly IRepository<LocationEntity> locationRepository;
        readonly IRepository<TitleEntity> titleRepository;

        readonly ConcurrentDictionary<string, IDictionary<string, string>> languageGameIdsCache;

        IDictionary<string, Location> locations;
        IDictionary<string, Language> languages;
        IDictionary<string, Title> titles;

        public LocalisationFetcher(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            IRepository<TitleEntity> titleRepository)
        {
            this.languageRepository = languageRepository;
            this.locationRepository = locationRepository;
            this.titleRepository = titleRepository;

            languageGameIdsCache = new ConcurrentDictionary<string, IDictionary<string, string>>();

            LoadData();
        }

        void LoadData()
        {
            locations = locationRepository
                .GetAll()
                .ToServiceModels()
                .ToDictionary(key => key.Id, val => val);

            languages = languageRepository
                .GetAll()
                .ToServiceModels()
                .ToDictionary(key => key.Id, val => val);

            titles = titleRepository
                .GetAll()
                .ToServiceModels()
                .ToDictionary(key => key.Id, val => val);
        }

        public IEnumerable<Localisation> GetGameLocationLocalisations(string locationGameId, string game)
        {
            ConcurrentBag<Localisation> localisations = new ConcurrentBag<Localisation>();

            Location location = locations.Values.FirstOrDefault(x => x.GameIds.Any(x => x.Game == game && x.Id == locationGameId));

            if (location is null)
            {
                return localisations;
            }

            IDictionary<string, string> languageGameIds = GetLanguageGameIds(game);
            
            Parallel.ForEach(languageGameIds, languageGameId => 
            {
                Localisation localisation = GetLocationLocalisation(location, languageGameId.Value);

                if (localisation is null)
                {
                    return;
                }
                
                localisation.GameId = locationGameId;
                localisation.LanguageGameId = languageGameId.Key;

                localisations.Add(localisation);
            });

            return localisations;
        }

        public Localisation GetTitleLocalisation(string titleId, string languageGameId, string game)
        {
            IList<Localisation> localisations = new List<Localisation>();
            Title title = titles[titleId];

            if (title.IsEmpty())
            {
                return null;
            }

            Language language = languages.Values.
                First(lang => lang.GameIds.Any(langGameId =>
                    langGameId.Game == game &&
                    langGameId.Id == languageGameId));

            List<string> titleIdsToCheck = new List<string>() { title.Id };
            List<string> languageIdsToCheck = new List<string>() { language.Id };

            titleIdsToCheck.AddRange(title.FallbackTitles);
            languageIdsToCheck.AddRange(language.FallbackLanguages);

            foreach (string titleIdToCheck in titleIdsToCheck)
            {
                foreach (string languageIdToCheck in languageIdsToCheck)
                {
                    foreach (Name name in titles[titleIdToCheck].Names)
                    {
                        if (name.LanguageId == languageIdToCheck)
                        {
                            Localisation localisation = new Localisation();
                            localisation.Id = titleIdToCheck;
                            localisation.LanguageId = languageIdToCheck;
                            localisation.Name = name.Value;
                            localisation.Adjective = name.Adjective;
                            localisation.Comment = name.Comment;

                            return localisation;
                        }
                    }
                }
            }

            return null;
        }

        Localisation GetLocationLocalisation(Location location, string languageId)
        {
            if (location.IsEmpty())
            {
                return null;
            }

            Language language = languages[languageId];

            List<string> locationIdsToCheck = new List<string>() { location.Id };
            List<string> languageIdsToCheck = new List<string>() { language.Id };

            locationIdsToCheck.AddRange(location.FallbackLocations);
            languageIdsToCheck.AddRange(language.FallbackLanguages);

            foreach (string locationIdToCheck in locationIdsToCheck)
            {
                foreach (string languageIdToCheck in languageIdsToCheck)
                {
                    foreach (Name name in locations[locationIdToCheck].Names)
                    {
                        if (name.LanguageId == languageIdToCheck)
                        {
                            Localisation localisation = new Localisation();
                            localisation.Id = locationIdToCheck;
                            localisation.LanguageId = languageIdToCheck;
                            localisation.Name = name.Value;
                            localisation.Adjective = name.Adjective;
                            localisation.Comment = name.Comment;

                            return localisation;
                        }
                    }
                }
            }

            return null;
        }

        IDictionary<string, string> GetLanguageGameIds(string game)
        {
            if (languageGameIdsCache.ContainsKey(game))
            {
                return languageGameIdsCache[game];
            }

            IDictionary<string, string> languageGameIds = languages.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == game)
                .ToDictionary(
                    key => key.Id,
                    val => languages.Values.First(language => language.GameIds.Any(x => x.Game == game && x.Id == val.Id)).Id);

            languageGameIdsCache.TryAdd(game, languageGameIds);

            return languageGameIds;
        }
    }
}
