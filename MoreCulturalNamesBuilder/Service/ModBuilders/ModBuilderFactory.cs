using System;

using Microsoft.Extensions.DependencyInjection;
using NuciDAL.Repositories;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class ModBuilderFactory : IModBuilderFactory
    {
        readonly IServiceProvider serviceProvider;

        public ModBuilderFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IModBuilder GetModBuilder(Settings settings)
        {
            string normalisedGame = settings.Mod.Game.ToUpperInvariant().Trim();

            ILocalisationFetcher localisationFetcher = Program.ServiceProvider.GetService<ILocalisationFetcher>();
            INameNormaliser nameNormaliser = Program.ServiceProvider.GetService<INameNormaliser>();
            IRepository<LanguageEntity> languageRepository = Program.ServiceProvider.GetService<IRepository<LanguageEntity>>();
            IRepository<LocationEntity> locationRepository = Program.ServiceProvider.GetService<IRepository<LocationEntity>>();
            IRepository<TitleEntity> titleRepository = Program.ServiceProvider.GetService<IRepository<TitleEntity>>();

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
                if (settings.Mod.GameVersion.StartsWith("1.4"))
                {
                    return new CK3v14ModBuilder(
                        localisationFetcher,
                        nameNormaliser,
                        languageRepository,
                        locationRepository,
                        titleRepository,
                        settings);
                }
                else
                {
                    return new CK3ModBuilder(
                        localisationFetcher,
                        nameNormaliser,
                        languageRepository,
                        locationRepository,
                        titleRepository,
                        settings);
                }
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
                    nameNormaliser,
                    languageRepository,
                    locationRepository,
                    titleRepository,
                    settings);
            }
            
            throw new NotImplementedException($"The game \"{settings.Mod.Game}\" is not supported");
        }
    }
}
