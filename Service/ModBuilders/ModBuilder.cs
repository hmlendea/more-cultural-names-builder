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
        public virtual string Game { get; protected set; }

        public string ModId => outputSettings.GetModId(Game);

        protected string OutputDirectoryPath => Path.Combine(outputSettings.ModOutputDirectory, Game);

        protected readonly IRepository<LanguageEntity> languageRepository;
        protected readonly IRepository<LocationEntity> locationRepository;

        protected readonly OutputSettings outputSettings;
        
        protected IDictionary<string, Location> locations;
        protected IDictionary<string, Language> languages;

        protected IEnumerable<GameId> locationGameIds;
        protected IEnumerable<GameId> languageGameIds;

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

            Console.WriteLine($" > Building the mod for {Game}...");

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
                .OrderBy(x => x.Id)
                .ToDictionary(key => key.Id, val => val);

            languages = languageRepository
                .GetAll()
                .ToServiceModels()
                .OrderBy(x => x.Id)
                .ToDictionary(key => key.Id, val => val);

            locationGameIds = locations.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Game)
                .OrderBy(x => x.Id)
                .ToList();

            languageGameIds = languages.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Game)
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
