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
            Language serviceModel = new Language();
            serviceModel.Id = dataObject.Id;
            serviceModel.Code = dataObject.Code?.ToServiceModel();
            serviceModel.GameIds = dataObject.GameIds.ToServiceModels();
            serviceModel.FallbackLanguages = dataObject.FallbackLanguages.ToList();

            return serviceModel;
        }

        internal static LanguageEntity ToDataObject(this Language serviceModel)
        {
            LanguageEntity dataObject = new LanguageEntity();
            dataObject.Id = serviceModel.Id;
            dataObject.Code = serviceModel.Code?.ToDataObject();
            dataObject.GameIds = serviceModel.GameIds.ToDataObjects().ToList();
            dataObject.FallbackLanguages = serviceModel.FallbackLanguages.ToList();

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
