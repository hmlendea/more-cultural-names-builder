using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    internal static class LanguageMappingExtensions
    {
        internal static Language ToServiceModel(this LanguageDataObject dataObject) => new()
        {
            Id = dataObject.Id,
            Code = dataObject.Code?.ToServiceModel(),
            GameIds = dataObject.GameIds.ToServiceModels(),
            FallbackLanguages = dataObject.FallbackLanguages.ToList()
        };

        internal static LanguageDataObject ToDataObject(this Language serviceModel) => new()
        {
            Id = serviceModel.Id,
            Code = serviceModel.Code?.ToDataObject(),
            GameIds = [.. serviceModel.GameIds.ToDataObjects()],
            FallbackLanguages = [.. serviceModel.FallbackLanguages]
        };

        internal static IEnumerable<Language> ToServiceModels(this IEnumerable<LanguageDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        internal static IEnumerable<LanguageDataObject> ToDataObjects(this IEnumerable<Language> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
