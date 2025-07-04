using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuciDAL.Repositories;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Mapping;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public abstract class ModBuilder(
        IFileRepository<LanguageEntity> languageRepository,
        IFileRepository<LocationEntity> locationRepository,
        Settings settings) : IModBuilder
    {
        protected string OutputDirectoryPath => Path.Combine(Settings.Output.ModOutputDirectory, Settings.Mod.Game);

        protected readonly IFileRepository<LanguageEntity> languageRepository = languageRepository;
        protected readonly IFileRepository<LocationEntity> locationRepository = locationRepository;

        protected readonly Settings Settings = settings;

        protected IDictionary<string, Language> languages;
        protected IDictionary<string, Location> locations;

        protected IEnumerable<GameId> languageGameIds;
        protected IEnumerable<GameId> locationGameIds;
        protected IEnumerable<GameId> titleGameIds;

        public void Build()
        {
            Console.WriteLine($" > Building the mod for {Settings.Mod.Game}...");

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

            locationGameIds = locations.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Settings.Mod.Game)
                .OrderBy(x => x.Id)
                .ToList();

            languageGameIds = languages.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Settings.Mod.Game)
                .OrderBy(x => x.Id)
                .ToList();

            LoadData();
        }

        static void StartTimedOperation(string message, Action operation)
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
