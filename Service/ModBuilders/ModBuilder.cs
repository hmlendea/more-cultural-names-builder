using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Mapping;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders
{
    public abstract class ModBuilder : IModBuilder
    {
        protected string OutputDirectoryPath => Path.Combine(outputSettings.ModOutputDirectory, modSettings.Game);

        protected readonly IRepository<LanguageEntity> languageRepository;
        protected readonly IRepository<LocationEntity> locationRepository;
        protected readonly IRepository<TitleEntity> titleRepository;

        protected readonly ModSettings modSettings;
        protected readonly OutputSettings outputSettings;
        
        protected IDictionary<string, Location> locations;
        protected IDictionary<string, Language> languages;
        protected IDictionary<string, Title> titles;

        protected IEnumerable<GameId> locationGameIds;
        protected IEnumerable<GameId> languageGameIds;
        protected IEnumerable<GameId> titleGameIds;

        public ModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            IRepository<TitleEntity> titleRepository,
            ModSettings modSettings,
            OutputSettings outputSettings)
        {
            this.languageRepository = languageRepository;
            this.locationRepository = locationRepository;
            this.titleRepository = titleRepository;

            this.modSettings = modSettings;
            this.outputSettings = outputSettings;
        }

        public void Build()
        {

            Console.WriteLine($" > Building the mod for {modSettings.Game}...");

            StartTimedOperation("Fetching the data", () => LoadAllData());
            StartTimedOperation("Generating the files", () => GenerateFiles());
        }

        protected abstract void GenerateFiles();

        protected abstract void LoadData();

        void LoadAllData()
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

            locationGameIds = locations.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == modSettings.Game)
                .OrderBy(x => x.Id)
                .ToList();

            languageGameIds = languages.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == modSettings.Game)
                .OrderBy(x => x.Id)
                .ToList();

            titleGameIds = titles.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == modSettings.Game)
                .OrderBy(x => x.Id)
                .ToList();

            LoadData();
        }

        void StartTimedOperation(string message, Action operation)
        {
            Console.Write($"   > {message}...");

            DateTime start = DateTime.Now;
            operation();
            DateTime finish = DateTime.Now;

            TimeSpan duration = finish - start;

            Console.WriteLine($" (Finished in {Math.Round(duration.TotalSeconds)}s)");
        }
    }
}
