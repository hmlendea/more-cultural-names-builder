using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    static class LanguageCodeMapping
    {
        internal static LanguageCode ToServiceModel(this LanguageCodeEntity dataObject)
        {
            LanguageCode serviceModel = new()
            {
                ISO_639_1 = dataObject.ISO_639_1,
                ISO_639_2 = dataObject.ISO_639_2,
                ISO_639_3 = dataObject.ISO_639_3
            };

            return serviceModel;
        }

        internal static LanguageCodeEntity ToDataObject(this LanguageCode serviceModel)
        {
            LanguageCodeEntity dataObject = new()
            {
                ISO_639_1 = serviceModel.ISO_639_1,
                ISO_639_2 = serviceModel.ISO_639_2,
                ISO_639_3 = serviceModel.ISO_639_3
            };

            return dataObject;
        }

        internal static IEnumerable<LanguageCode> ToServiceModels(this IEnumerable<LanguageCodeEntity> dataObjects)
        {
            IEnumerable<LanguageCode> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }

        internal static IEnumerable<LanguageCodeEntity> ToDataObjects(this IEnumerable<LanguageCode> serviceModels)
        {
            IEnumerable<LanguageCodeEntity> dataObjects = serviceModels.Select(serviceModel => serviceModel.ToDataObject());

            return dataObjects;
        }
    }
}
