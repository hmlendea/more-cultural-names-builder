using System;

using Microsoft.Extensions.DependencyInjection;

using NuciDAL.Repositories;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service;
using MoreCulturalNamesBuilder.Service.ModBuilders;
using MoreCulturalNamesBuilder.Service.ModBuilders.ImperatorRome;

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
                .AddSingleton<IRepository<LanguageEntity>>(s => new XmlRepository<LanguageEntity>(settings.Input.LanguageStorePath))
                .AddSingleton<IRepository<LocationEntity>>(s => new XmlRepository<LocationEntity>(settings.Input.LocationStorePath))
                .AddSingleton<ILocalisationFetcher, LocalisationFetcher>()
                .AddSingleton<INameNormaliser, NameNormaliser>()
                .AddSingleton<IModBuilderFactory, ModBuilderFactory>()
                .AddSingleton<IImperatorRomeDescriptorBuilder, ImperatorRomeDescriptorBuilder>()
                .AddSingleton<IImperatorRomeLocalisationBuilder, ImperatorRomeLocalisationBuilder>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
