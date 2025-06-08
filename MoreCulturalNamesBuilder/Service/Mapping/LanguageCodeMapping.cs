using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    static class LanguageCodeMapping
    {
        internal static LanguageCode ToServiceModel(this LanguageCodeEntity dataObject) => new()
        {
            ISO_639_1 = dataObject.ISO_639_1,
            ISO_639_2 = dataObject.ISO_639_2,
            ISO_639_3 = dataObject.ISO_639_3
        };

        internal static LanguageCodeEntity ToDataObject(this LanguageCode serviceModel) => new()
        {
            ISO_639_1 = serviceModel.ISO_639_1,
            ISO_639_2 = serviceModel.ISO_639_2,
            ISO_639_3 = serviceModel.ISO_639_3
        };

        internal static IEnumerable<LanguageCode> ToServiceModels(this IEnumerable<LanguageCodeEntity> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        internal static IEnumerable<LanguageCodeEntity> ToDataObjects(this IEnumerable<LanguageCode> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
