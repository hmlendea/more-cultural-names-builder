using System;

using NuciDAL.Repositories;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class ModBuilderFactory(
        ILocalisationFetcher localisationFetcher,
        INameNormaliser nameNormaliser,
        IFileRepository<LanguageDataObject> languageRepository,
        IFileRepository<LocationDataObject> locationRepository) : IModBuilderFactory
    {
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

            if (normalisedGame.StartsWith("CIV5") ||
                normalisedGame.StartsWith("CIVILIZATION5"))
            {
                return new Civilization5ModBuilder(
                    localisationFetcher,
                    nameNormaliser,
                    languageRepository,
                    locationRepository,
                    settings);
            }

            if (normalisedGame.StartsWith("EU5") ||
                normalisedGame.StartsWith("EUROPAUNIVERSALIS5"))
            {
                return new EuropaUniversalis5ModBuilder(
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

            if (normalisedGame.StartsWith("VIC3") ||
                normalisedGame.StartsWith("VICTORIA3"))
            {
                return new Victoria3ModBuilder(
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
