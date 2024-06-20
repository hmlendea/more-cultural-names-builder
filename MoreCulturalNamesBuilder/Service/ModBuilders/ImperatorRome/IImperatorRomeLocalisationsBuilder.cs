using System.Collections.Generic;

using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.ModBuilders.ImperatorRome
{
    public interface IImperatorRomeLocalisationsBuilder
    {
        void CreateLocalisationFiles(
            string localisationDirectoryPath,
            IDictionary<string, IDictionary<string, Localisation>> localisations,
            IEnumerable<GameId> locationGameIds);
    }
}
