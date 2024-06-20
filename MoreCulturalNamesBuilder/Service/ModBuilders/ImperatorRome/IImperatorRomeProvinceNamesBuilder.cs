using MoreCulturalNamesBuilder.Service.Models;
using System.Collections.Generic;

namespace MoreCulturalNamesBuilder.Service.ModBuilders.ImperatorRome
{
    public interface IImperatorRomeProvinceNamesBuilder
    {
        public void CreateProvinceNameFiles(
            string provinceNamesDirectoryPath,
            IDictionary<string, IDictionary<string, Localisation>> localisations,
            IEnumerable<GameId> languageGameIds);
    }
}
