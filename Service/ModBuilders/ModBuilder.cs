using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Mapping;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders
{
    public abstract class ModBuilder : IModBuilder
    {
        public virtual string Game { get; protected set; }

        protected string OutputDirectoryPath => Path.Combine(outputSettings.ModOutputDirectory, Game);

        protected readonly IRepository<LanguageEntity> languageRepository;
        protected readonly IRepository<LocationEntity> locationRepository;

        protected readonly OutputSettings outputSettings;

        public ModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
        {
            this.languageRepository = languageRepository;
            this.locationRepository = locationRepository;

            this.outputSettings = outputSettings;
        }

        public void Build()
        {
            Console.WriteLine($"Started building for {Game}...");

            BuildMod();

            Console.WriteLine($"Finished building for {Game}!");
        }

        protected virtual void BuildMod()
        {

        }

        protected List<Localisation> GetLocalisations()
        {
            IEnumerable<Location> locations = locationRepository.GetAll().ToServiceModels();
            IEnumerable<Language> languages = languageRepository.GetAll().ToServiceModels();

            List<Localisation> localisations = new List<Localisation>();

            foreach (Location location in locations.Where(x => x.GameIds.Any(y => y.Game == Game)))
            {
                List<Localisation> locationLocalisations = GetLocationLocalisations(location.Id);
                localisations.AddRange(locationLocalisations);
            }

            return localisations
                .OrderBy(x => x.LocationId.PadLeft(64, ' '))
                .ThenBy(x => x.LanguageId)
                .ToList();
        }

        protected virtual List<Localisation> GetGameLocationLocalisations(string locationGameId)
        {
            List<Localisation> localisations = new List<Localisation>();
            IEnumerable<Location> locations = locationRepository.GetAll().ToServiceModels();
            IEnumerable<Language> languages = languageRepository.GetAll().ToServiceModels();
            Location location = locations.First(x => x.GameIds.Any(y => y.Id == locationGameId));

            IEnumerable<GameId> languageGameIds = languages
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Game)
                .ToList();

            foreach (GameId languageGameId in languageGameIds)
            {
                string name = GetLocationName(languages, location, languageGameId.Id);
                
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                Localisation localisation = new Localisation();
                localisation.LocationId = locationGameId;
                localisation.LanguageId = languageGameId.Id;
                localisation.Name = name;

                localisations.Add(localisation);
            }

            return localisations;
        }

        protected virtual List<Localisation> GetLocationLocalisations(string locationId)
        {
            List<Localisation> localisations = new List<Localisation>();
            Location location = locationRepository.Get(locationId).ToServiceModel();
            IEnumerable<Language> languages = languageRepository.GetAll().ToServiceModels();

            foreach (Language language in languages.Where(x => x.GameIds.Any(y => y.Game == Game)))
            {
                List<string> languagesToCheck = new List<string>() { language.Id };
                languagesToCheck.AddRange(language.FallbackLanguages);

                foreach (string languageIdToCheck in languagesToCheck)
                {
                    LocationName locationName = location.Names.FirstOrDefault(x => x.LanguageId == languageIdToCheck);

                    if (!(locationName is null))
                    {
                        foreach (GameId locationGameId in location.GameIds.Where(x => x.Game == Game))
                        {
                            foreach (GameId languageGameId in language.GameIds.Where(x => x.Game == Game))
                            {
                                Localisation localisation = new Localisation();
                                localisation.LocationId = locationGameId.Id;
                                localisation.LanguageId = languageGameId.Id;
                                localisation.Name = locationName.Value;

                                localisations.Add(localisation);
                            }
                        }

                        break;
                    }
                }
            }

            return localisations;
        }

        protected string GetWindows1252Name(string name)
        {
            string processedName = name;

            processedName = Regex.Replace(processedName, "[ĂĀ]", "Ã");
            processedName = Regex.Replace(processedName, "[ḂḄ]", "B");
            processedName = Regex.Replace(processedName, "[ČĆ]", "C");
            processedName = Regex.Replace(processedName, "[ĐƊḌ]", "D");
            processedName = Regex.Replace(processedName, "[Ē]", "Ë");
            processedName = Regex.Replace(processedName, "[Ę]", "E");
            processedName = Regex.Replace(processedName, "[Ğ]", "G");
            processedName = Regex.Replace(processedName, "[İĪ]", "I");
            processedName = Regex.Replace(processedName, "[Ƙ]", "K");
            processedName = Regex.Replace(processedName, "[Ł]", "L");
            processedName = Regex.Replace(processedName, "[Ń]", "N");
            processedName = Regex.Replace(processedName, "[Ō]", "Ö");
            processedName = Regex.Replace(processedName, "[ȘŞṢŚŠ]", "S");
            processedName = Regex.Replace(processedName, "[ȚТ]", "T");
            processedName = Regex.Replace(processedName, "[Ť]", "Ty");
            processedName = Regex.Replace(processedName, "[Ū]", "Ü");
            processedName = Regex.Replace(processedName, "[Ư]", "U'");
            processedName = Regex.Replace(processedName, "[ŹŻŽ]", "Z");
            processedName = Regex.Replace(processedName, "[ăąā]", "ã");
            processedName = Regex.Replace(processedName, "[ḃḅ]", "b");
            processedName = Regex.Replace(processedName, "[ć]", "c");
            processedName = Regex.Replace(processedName, "[č]", "ch");
            processedName = Regex.Replace(processedName, "[đɗḍ]", "d");
            processedName = Regex.Replace(processedName, "[ě]", "ie");
            processedName = Regex.Replace(processedName, "[ē]", "ë");
            processedName = Regex.Replace(processedName, "[ęе]", "e");
            processedName = Regex.Replace(processedName, "[ğ]", "g");
            processedName = Regex.Replace(processedName, "[īı]", "i");
            processedName = Regex.Replace(processedName, "[ƙк]", "k");
            processedName = Regex.Replace(processedName, "[ł]", "l");
            processedName = Regex.Replace(processedName, "[ń]", "n");
            processedName = Regex.Replace(processedName, "[ō]", "ö");
            processedName = Regex.Replace(processedName, "[ř]", "rz");
            processedName = Regex.Replace(processedName, "[șşṣśš]", "s");
            processedName = Regex.Replace(processedName, "[ț]", "t");
            processedName = Regex.Replace(processedName, "[ū]", "ü");
            processedName = Regex.Replace(processedName, "[źżž]", "z");

            processedName = Regex.Replace(processedName, "[ʻ]", "'");

            return processedName;
        }

        string GetLocationName(IEnumerable<Language> languages, Location location, string languageGameId)
        {
            Language language = languages.First(x => x.GameIds.Any(x => x.Id == languageGameId));
            List<string> languagesToCheck = new List<string>() { language.Id };
            languagesToCheck.AddRange(language.FallbackLanguages);

            foreach (string languageIdToCheck in languagesToCheck)
            {
                foreach (LocationName name in location.Names)
                {
                    if (name.LanguageId == languageIdToCheck)
                    {
                        return name.Value;
                    }
                }
            }

            return null;
        }


        protected virtual string GetName(string locationId, string languageId)
        {
            Location location = locationRepository.Get(locationId).ToServiceModel();
            Language language = languageRepository.Get(languageId).ToServiceModel();

            List<string> languagesToCheck = new List<string>() { language.Id };
            languagesToCheck.AddRange(language.FallbackLanguages);

            return location.Names.FirstOrDefault(x => languagesToCheck.Contains(x.LanguageId)).Value;
        }
    }
}
