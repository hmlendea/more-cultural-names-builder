using System;

using Microsoft.Extensions.DependencyInjection;

using NuciDAL.Repositories;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service;
using MoreCulturalNamesBuilder.Service.ModBuilders;

namespace MoreCulturalNamesBuilder
{
    public class Program
    {
        public static IServiceProvider ServiceProvider;

        static Settings settings = null;

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            settings = new Settings(args);
            BuildServiceProvider();

            ServiceProvider
                .GetService<IModBuilderFactory>()
                .GetModBuilder(settings)
                .Build();
        }

        static void BuildServiceProvider()
        {
            IServiceCollection serviceCollection = new ServiceCollection()
                .AddSingleton(settings)
                .AddSingleton<IFileRepository<LanguageEntity>>(s => new XmlRepository<LanguageEntity>(settings.Input.LanguageStorePath))
                .AddSingleton<IFileRepository<LocationEntity>>(s => new XmlRepository<LocationEntity>(settings.Input.LocationStorePath))
                .AddSingleton<ILocalisationFetcher, LocalisationFetcher>()
                .AddSingleton<INameNormaliser, NameNormaliser>()
                .AddSingleton<IModBuilderFactory, ModBuilderFactory>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
