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

        readonly IDictionary<string, Location> locations;
        readonly IDictionary<string, Language> languages;

        public LocalisationFetcher(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository)
        {
            this.languageRepository = languageRepository;
            this.locationRepository = locationRepository;

            locations = locationRepository
                .GetAll()
                .ToServiceModels()
                .ToDictionary(key => key.Id, val => val);

            languages = languageRepository
                .GetAll()
                .ToServiceModels()
                .ToDictionary(key => key.Id, val => val);
        }

        public IEnumerable<Localisation> GetGameLocationLocalisations(string locationGameId, string gameId)
        {
            ConcurrentBag<Localisation> localisations = new ConcurrentBag<Localisation>();

            IDictionary<string, string> languageGameIds = languages.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == gameId)
                .ToDictionary(
                    key => key.Id,
                    val => languages.Values.First(language => language.GameIds.Any(x => x.Game == gameId && x.Id == val.Id)).Id);

            Location location = locations.Values.FirstOrDefault(x => x.GameIds.Any(x => x.Game == gameId && x.Id == locationGameId));

            if (location is null)
            {
                return localisations;
            }
            
            Parallel.ForEach(languageGameIds, gameLanguage => 
            {
                string name = GetLocationName(location, gameLanguage.Value);
                
                if (!string.IsNullOrWhiteSpace(name))
                {
                    Localisation localisation = new Localisation();
                    localisation.LocationId = location.Id;
                    localisation.LocationGameId = locationGameId;
                    localisation.LanguageId = gameLanguage.Value;
                    localisation.LanguageGameId = gameLanguage.Key;
                    localisation.Name = name;

                    localisations.Add(localisation);
                }
            });

            return localisations;
        }

        string GetLocationName(Location location, string languageId)
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
                    foreach (LocationName name in locations[locationIdToCheck].Names)
                    {
                        if (name.LanguageId == languageIdToCheck)
                        {
                            return name.Value;
                        }
                    }
                }
            }

            return null;
        }
    }
}
