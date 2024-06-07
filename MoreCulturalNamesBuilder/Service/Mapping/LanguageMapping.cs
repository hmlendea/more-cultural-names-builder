using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    static class LanguageMapping
    {
        internal static Language ToServiceModel(this LanguageEntity dataObject)
        {
            Language serviceModel = new()
            {
                Id = dataObject.Id,
                Code = dataObject.Code?.ToServiceModel(),
                GameIds = dataObject.GameIds.ToServiceModels(),
                FallbackLanguages = dataObject.FallbackLanguages.ToList()
            };

            return serviceModel;
        }

        internal static LanguageEntity ToDataObject(this Language serviceModel)
        {
            LanguageEntity dataObject = new()
            {
                Id = serviceModel.Id,
                Code = serviceModel.Code?.ToDataObject(),
                GameIds = serviceModel.GameIds.ToDataObjects().ToList(),
                FallbackLanguages = serviceModel.FallbackLanguages.ToList()
            };

            return dataObject;
        }

        internal static IEnumerable<Language> ToServiceModels(this IEnumerable<LanguageEntity> dataObjects)
        {
            IEnumerable<Language> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }

        internal static IEnumerable<LanguageEntity> ToDataObjects(this IEnumerable<Language> serviceModels)
        {
            IEnumerable<LanguageEntity> dataObjects = serviceModels.Select(serviceModel => serviceModel.ToDataObject());

            return dataObjects;
        }
    }
}
