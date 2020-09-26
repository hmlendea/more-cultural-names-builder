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
using MoreCulturalNamesModBuilder.Service.ModBuilders.ImperatorRome;

namespace MoreCulturalNamesModBuilder
{
    public class Program
    {
        static DataStoreSettings dataStoreSettings = null;
        static OutputSettings outputSettings = null;

        static string[] LanguageStorePathOptions = { "-l", "--languages" };
        static string[] TitlesStorePathOptions = { "-t", "--titles" };
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
                .AddSingleton(dataStoreSettings)
                .AddSingleton(outputSettings)
                .AddSingleton<IRepository<LanguageEntity>>(s => new XmlRepository<LanguageEntity>(dataStoreSettings.LanguageStorePath))
                .AddSingleton<IRepository<LocationEntity>>(s => new XmlRepository<LocationEntity>(dataStoreSettings.TitleStorePath))
                .AddSingleton<ILocalisationFetcher, LocalisationFetcher>()
                .AddSingleton<INameNormaliser, NameNormaliser>()
                .AddSingleton<ICK2ModBuilder, CK2ModBuilder>()
                .AddSingleton<ICK2HIPModBuilder, CK2HIPModBuilder>()
                .AddSingleton<ICK3ModBuilder, CK3ModBuilder>()
                .AddSingleton<IImperatorRomeModBuilder, ImperatorRomeModBuilder>()
                .BuildServiceProvider();
            
            IModBuilder ck2Builder = serviceProvider.GetService<ICK2ModBuilder>();
            IModBuilder ck2hipBuilder = serviceProvider.GetService<ICK2HIPModBuilder>();
            IModBuilder ck3Builder = serviceProvider.GetService<ICK3ModBuilder>();
            IModBuilder imperatorRomeBuilder = serviceProvider.GetService<IImperatorRomeModBuilder>();
            
            ck2Builder.Build();
            ck2hipBuilder.Build();
            ck3Builder.Build();
            imperatorRomeBuilder.Build();
        }

        static void LoadConfiguration(string[] args)
        {
            IConfiguration config =new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            dataStoreSettings = new DataStoreSettings();
            outputSettings = new OutputSettings();

            config.Bind(nameof(DataStoreSettings), dataStoreSettings);
            config.Bind(nameof(OutputSettings), outputSettings);

            dataStoreSettings.LanguageStorePath = CliArgumentsReader.GetOptionValue(args, LanguageStorePathOptions);
            dataStoreSettings.TitleStorePath = CliArgumentsReader.GetOptionValue(args, TitlesStorePathOptions);
            outputSettings.ModVersion = CliArgumentsReader.GetOptionValue(args, VersionOptions);
            outputSettings.ModOutputDirectory = CliArgumentsReader.GetOptionValue(args, OutputDirectoryPathOptions);
        }
    }
}
