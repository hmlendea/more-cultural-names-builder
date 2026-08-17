using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.UnitTests.Service.ModBuilders
{
    internal static class ModBuilderTestDataFactory
    {
        internal static LanguageEntity CreateLanguage(
            string game,
            string languageId,
            string languageGameId)
            => new()
            {
                Id = languageId,
                Code = null,
                GameIds = [new(game, languageGameId)],
                FallbackLanguages = []
            };

        internal static LocationEntity CreateLocation(
            string game,
            string locationId,
            string locationGameId,
            string locationGameIdType,
            string defaultNameLanguageId)
            => new()
            {
                Id = locationId,
                GeoNamesId = null,
                GameIds =
                [
                    new(game, locationGameId)
                    {
                        Type = locationGameIdType,
                        DefaultNameLanguageId = defaultNameLanguageId
                    }
                ],
                FallbackLocations = [],
                Names = []
            };

        internal static Localisation CreateLocalisation(
            string id,
            string gameId,
            string languageId,
            string languageGameId,
            string name,
            string adjective,
            string comment)
            => new()
            {
                Id = id,
                GameId = gameId,
                LanguageId = languageId,
                LanguageGameId = languageGameId,
                Name = name,
                Adjective = adjective,
                Comment = comment
            };
    }
}