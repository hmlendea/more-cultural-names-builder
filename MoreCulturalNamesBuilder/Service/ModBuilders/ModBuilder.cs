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
        IFileRepository<LanguageDataObject> languageRepository,
        IFileRepository<LocationDataObject> locationRepository,
        Settings settings) : IModBuilder
    {
        protected readonly IFileRepository<LanguageDataObject> languageRepository = languageRepository;
        protected readonly IFileRepository<LocationDataObject> locationRepository = locationRepository;

        protected readonly Settings Settings = settings;

        protected IDictionary<string, Language> languages = null!;
        protected IDictionary<string, Location> locations = null!;

        protected IEnumerable<GameId> languageGameIds = null!;
        protected IEnumerable<GameId> locationGameIds = null!;
        protected IEnumerable<GameId> titleGameIds = null!;

        protected string OutputDirectoryPath => Path.Combine(Settings.Output.ModOutputDirectory, Settings.Mod.Game);

        public void Build()
        {
            Console.WriteLine($" > Building the mod for {Settings.Mod.Game}...");

            StartTimedOperation("Fetching the data", () => LoadAllData());
            StartTimedOperation("Generating the files", () => GenerateFiles());
        }

        protected abstract void GenerateFiles();

        protected abstract void LoadData();

        private void LoadAllData()
        {
            locations = locationRepository
                .GetAll()
                .ToServiceModels()
                .ToDictionary(location => location.Id, location => location);

            languages = languageRepository
                .GetAll()
                .ToServiceModels()
                .ToDictionary(language => language.Id, language => language);

            locationGameIds = locations.Values
                .SelectMany(location => location.GameIds)
                .Where(gameId => string.Equals(gameId.Game, Settings.Mod.Game))
                .OrderBy(gameId => gameId.Id)
                .ToList();

            languageGameIds = languages.Values
                .SelectMany(language => language.GameIds)
                .Where(gameId => string.Equals(gameId.Game, Settings.Mod.Game))
                .OrderBy(gameId => gameId.Id)
                .ToList();

            LoadData();
        }

        private static void StartTimedOperation(string message, Action operation)
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
