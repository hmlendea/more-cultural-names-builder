using System.Collections.Generic;

using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service
{
    public interface ILocalisationFetcher
    {
        IEnumerable<Localisation> GetGameLocationLocalisations(string locationGameId, string gameId);
    }
}
