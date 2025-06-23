using System;

using NuciDAL.Repositories;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class ModBuilderFactory(
        ILocalisationFetcher localisationFetcher,
        INameNormaliser nameNormaliser,
        IFileRepository<LanguageEntity> languageRepository,
        IFileRepository<LocationEntity> locationRepository) : IModBuilderFactory
    {
        readonly ILocalisationFetcher localisationFetcher = localisationFetcher;
        readonly INameNormaliser nameNormaliser = nameNormaliser;
        readonly IFileRepository<LanguageEntity> languageRepository = languageRepository;
        readonly IFileRepository<LocationEntity> locationRepository = locationRepository;

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
                    settings);
            }

            if (normalisedGame.StartsWith("CK3"))
            {
                return new CK3ModBuilder(
                    localisationFetcher,
                    nameNormaliser,
                    languageRepository,
                    locationRepository,
                    settings);
            }

            if (normalisedGame.StartsWith("HOI4"))
            {
                return new HOI4ModBuilder(
                    localisationFetcher,
                    nameNormaliser,
                    languageRepository,
                    locationRepository,
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
                    settings);
            }

            throw new NotImplementedException($"The game \"{settings.Mod.Game}\" is not supported");
        }
    }
}
