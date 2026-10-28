using System;

using Microsoft.Extensions.DependencyInjection;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service;
using MoreCulturalNamesBuilder.Service.ModBuilders;

using NuciDAL.Repositories;

using NuciText.Conversion;

namespace MoreCulturalNamesBuilder
{
    public static class Program
    {
        public static IServiceProvider ServiceProvider = null!;

        private static Settings settings = null!;

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            settings = new(args);
            BuildServiceProvider();

            ServiceProvider
                .GetRequiredService<IModBuilderFactory>()
                .GetModBuilder(settings)
                .Build();
        }

        private static void BuildServiceProvider()
            => ServiceProvider = new ServiceCollection()
                .AddSingleton(settings)
                .AddSingleton<IFileRepository<LanguageDataObject>>(
                    serviceProvider => new XmlRepository<LanguageDataObject>(settings.Input.LanguageStorePath))
                .AddSingleton<IFileRepository<LocationDataObject>>(
                    serviceProvider => new XmlRepository<LocationDataObject>(settings.Input.LocationStorePath))
                .AddSingleton<ILocalisationFetcher, LocalisationFetcher>()
                .AddSingleton<INameNormaliser, NameNormaliser>()
                .AddSingleton<INuciTextConverter, NuciTextConverter>()
                .AddSingleton<IModBuilderFactory, ModBuilderFactory>()
                .BuildServiceProvider();
    }
}
