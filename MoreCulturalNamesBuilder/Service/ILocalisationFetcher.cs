using System.Collections.Generic;

using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service
{
    public interface ILocalisationFetcher
    {
        IEnumerable<Localisation> GetGameLocationLocalisations(string locationGameId, string gameId);

        IEnumerable<Localisation> GetGameLocationLocalisations(string locationGameId, string locationGameIdType, string gameId);

        Localisation GetTitleLocalisation(string titleId, string languageId, string game);
    }
}
