using System.Collections.Generic;

using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service
{
    public interface ILocalisationFetcher
    {
        public IEnumerable<Localisation> GetGameLocationLocalisations(
            string locationGameId,
            string game);

        public IEnumerable<Localisation> GetGameLocationLocalisations(
            string locationGameId,
            string? locationGameIdType,
            string game);
    }
}
