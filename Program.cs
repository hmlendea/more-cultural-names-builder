using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciCLI;
using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service;
using MoreCulturalNamesModBuilder.Service.ModBuilders;
using MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings2;
using MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings3;
using MoreCulturalNamesModBuilder.Service.ModBuilders.HeartsOfIron4;
using MoreCulturalNamesModBuilder.Service.ModBuilders.ImperatorRome;

namespace MoreCulturalNamesModBuilder
{
    public class Program
    {
        static BuildSettings buildSettings = null;
        static DataStoreSettings dataStoreSettings = null;
        static OutputSettings outputSettings = null;

        static string[] LanguageStorePathOptions = { "--lang", "--languages" };
        static string[] LocationsStorePathOptions = { "--loc", "--locations" };
        static string[] TitleStorePathOptions = { "-t", "--titles" };
        static string[] VersionOptions = { "-v", "--ver", "--version" };
        static string[] OutputDirectoryPathOptions = { "-o", "--out", "--output" };

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            LoadConfiguration(args);

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(buildSettings)
                .AddSingleton(dataStoreSettings)
                .AddSingleton(outputSettings)
                .AddSingleton<IRepository<LanguageEntity>>(s => new XmlRepository<LanguageEntity>(dataStoreSettings.LanguageStorePath))
                .AddSingleton<IRepository<LocationEntity>>(s => new XmlRepository<LocationEntity>(dataStoreSettings.LocationStorePath))
                .AddSingleton<IRepository<TitleEntity>>(s => new XmlRepository<TitleEntity>(dataStoreSettings.TitleStorePath))
                .AddSingleton<ILocalisationFetcher, LocalisationFetcher>()
                .AddSingleton<INameNormaliser, NameNormaliser>()
                .AddSingleton<ICK2ModBuilder, CK2ModBuilder>()
                .AddSingleton<ICK2HIPModBuilder, CK2HIPModBuilder>()
                .AddSingleton<ICK3ModBuilder, CK3ModBuilder>()
                .AddSingleton<IHOI4ModBuilder, HOI4ModBuilder>()
                .AddSingleton<IImperatorRomeModBuilder, ImperatorRomeModBuilder>()
                .AddSingleton<IModBuilderFactory, ModBuilderFactory>()
                .BuildServiceProvider();

            serviceProvider
                .GetService<IModBuilderFactory>()
                .GetModBuilder(buildSettings.Game)
                .Build();
        }

        static void LoadConfiguration(string[] args)
        {
            IConfiguration config =new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            buildSettings = new BuildSettings(args);
            dataStoreSettings = new DataStoreSettings();
            outputSettings = new OutputSettings();

            config.Bind(nameof(DataStoreSettings), dataStoreSettings);
            config.Bind(nameof(OutputSettings), outputSettings);

            dataStoreSettings.LanguageStorePath = CliArgumentsReader.GetOptionValue(args, LanguageStorePathOptions);
            dataStoreSettings.LocationStorePath = CliArgumentsReader.GetOptionValue(args, LocationsStorePathOptions);
            dataStoreSettings.TitleStorePath = CliArgumentsReader.GetOptionValue(args, TitleStorePathOptions);
            outputSettings.ModVersion = CliArgumentsReader.GetOptionValue(args, VersionOptions);
            outputSettings.ModOutputDirectory = CliArgumentsReader.GetOptionValue(args, OutputDirectoryPathOptions);
        }
    }
}
