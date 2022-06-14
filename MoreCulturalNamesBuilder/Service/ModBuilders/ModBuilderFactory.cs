using System;

using NuciDAL.Repositories;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class ModBuilderFactory : IModBuilderFactory
    {
        readonly ILocalisationFetcher localisationFetcher;
        readonly INameNormaliser nameNormaliser;
        readonly IRepository<LanguageEntity> languageRepository;
        readonly IRepository<LocationEntity> locationRepository;
        readonly IRepository<TitleEntity> titleRepository;

        public ModBuilderFactory(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            IRepository<TitleEntity> titleRepository)
        {
            this.localisationFetcher = localisationFetcher;
            this.nameNormaliser = nameNormaliser;
            this.languageRepository = languageRepository;
            this.locationRepository = locationRepository;
            this.titleRepository = titleRepository;
        }

        public IModBuilder GetModBuilder(Settings settings)
        {
            string normalisedGame = settings.Mod.Game.ToUpperInvariant().Trim();

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
                if (string.Compare(settings.Mod.GameVersion.Substring(0, 3), "1.4") <= 0)
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
