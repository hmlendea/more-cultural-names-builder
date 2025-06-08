using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    static class LanguageMapping
    {
        internal static Language ToServiceModel(this LanguageEntity dataObject) => new()
        {
            Id = dataObject.Id,
            Code = dataObject.Code?.ToServiceModel(),
            GameIds = dataObject.GameIds.ToServiceModels(),
            FallbackLanguages = dataObject.FallbackLanguages.ToList()
        };

        internal static LanguageEntity ToDataObject(this Language serviceModel) => new()
        {
            Id = serviceModel.Id,
            Code = serviceModel.Code?.ToDataObject(),
            GameIds = serviceModel.GameIds.ToDataObjects().ToList(),
            FallbackLanguages = serviceModel.FallbackLanguages.ToList()
        };

        internal static IEnumerable<Language> ToServiceModels(this IEnumerable<LanguageEntity> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        internal static IEnumerable<LanguageEntity> ToDataObjects(this IEnumerable<Language> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
