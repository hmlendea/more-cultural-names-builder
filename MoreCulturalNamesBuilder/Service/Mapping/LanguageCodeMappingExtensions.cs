using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    internal static class LanguageCodeMappingExtensions
    {
        internal static LanguageCode ToServiceModel(this LanguageCodeDataObject dataObject) => new()
        {
            ISO_639_1 = dataObject.ISO_639_1,
            ISO_639_2 = dataObject.ISO_639_2,
            ISO_639_3 = dataObject.ISO_639_3
        };

        internal static LanguageCodeDataObject ToDataObject(this LanguageCode serviceModel) => new()
        {
            ISO_639_1 = serviceModel.ISO_639_1,
            ISO_639_2 = serviceModel.ISO_639_2,
            ISO_639_3 = serviceModel.ISO_639_3
        };

        internal static IEnumerable<LanguageCode> ToServiceModels(this IEnumerable<LanguageCodeDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        internal static IEnumerable<LanguageCodeDataObject> ToDataObjects(this IEnumerable<LanguageCode> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
