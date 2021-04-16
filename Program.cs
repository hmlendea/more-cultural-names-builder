using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciCLI;
using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service;
using MoreCulturalNamesModBuilder.Service.ModBuilders;

namespace MoreCulturalNamesModBuilder
{
    public class Program
    {
        public static IServiceProvider ServiceProvider;

        static ModSettings modSettings = null;
        static InputSettings inputSettings = null;
        static OutputSettings outputSettings = null;

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            LoadConfiguration(args);
            BuildServiceProvider();

            ServiceProvider
                .GetService<IModBuilderFactory>()
                .GetModBuilder(modSettings.Game)
                .Build();
        }

        static void LoadConfiguration(string[] args)
        {
            modSettings = new ModSettings(args);
            inputSettings = new InputSettings(args);
            outputSettings = new OutputSettings(args);
        }

        static void BuildServiceProvider()
        {
            IServiceCollection serviceCollection = new ServiceCollection()
                .AddSingleton(modSettings)
                .AddSingleton(inputSettings)
                .AddSingleton(outputSettings)
                .AddSingleton<IRepository<LanguageEntity>>(s => new XmlRepository<LanguageEntity>(inputSettings.LanguageStorePath))
                .AddSingleton<IRepository<LocationEntity>>(s => new XmlRepository<LocationEntity>(inputSettings.LocationStorePath))
                .AddSingleton<IRepository<TitleEntity>>(s => new XmlRepository<TitleEntity>(inputSettings.TitleStorePath))
                .AddSingleton<ILocalisationFetcher, LocalisationFetcher>()
                .AddSingleton<INameNormaliser, NameNormaliser>()
                .AddSingleton<IModBuilderFactory, ModBuilderFactory>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
