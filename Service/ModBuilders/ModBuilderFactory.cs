using System;

using Microsoft.Extensions.DependencyInjection;
using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders
{
    public sealed class ModBuilderFactory : IModBuilderFactory
    {
        readonly IServiceProvider serviceProvider;

        public ModBuilderFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IModBuilder GetModBuilder(string game)
        {
            string normalisedGame = game.ToUpperInvariant().Trim();

            ILocalisationFetcher localisationFetcher = Program.ServiceProvider.GetService<ILocalisationFetcher>();
            INameNormaliser nameNormaliser = Program.ServiceProvider.GetService<INameNormaliser>();
            IRepository<LanguageEntity> languageRepository = Program.ServiceProvider.GetService<IRepository<LanguageEntity>>();
            IRepository<LocationEntity> locationRepository = Program.ServiceProvider.GetService<IRepository<LocationEntity>>();
            IRepository<TitleEntity> titleRepository = Program.ServiceProvider.GetService<IRepository<TitleEntity>>();
            Settings settings = Program.ServiceProvider.GetService<Settings>();

            if (normalisedGame.StartsWith("CK2"))
            {
                return new CK2ModBuilder(
                    localisationFetcher,
                    nameNormaliser,
                    languageRepository,
                    locationRepository,
                    titleRepository,
                    settings);
            }
            
            if (normalisedGame.StartsWith("CK3"))
            {
                return new CK3ModBuilder(
                    localisationFetcher,
                    nameNormaliser,
                    languageRepository,
                    locationRepository,
                    titleRepository,
                    settings);
            }
            
            if (normalisedGame.StartsWith("HOI4"))
            {
                return new HOI4ModBuilder(
                    localisationFetcher,
                    nameNormaliser,
                    languageRepository,
                    locationRepository,
                    titleRepository,
                    settings);
            }
            
            if (normalisedGame.StartsWith("IR") ||
                normalisedGame.StartsWith("IMPERATORROME"))
            {
                return new ImperatorRomeModBuilder(
                    localisationFetcher,
                    languageRepository,
                    locationRepository,
                    titleRepository,
                    settings);
            }
            
            throw new NotImplementedException($"The game \"{game}\" is not supported");
        }
    }
}
