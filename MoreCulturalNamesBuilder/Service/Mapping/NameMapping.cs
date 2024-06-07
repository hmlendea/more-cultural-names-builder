using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    static class NameMapping
    {
        internal static Name ToServiceModel(this NameEntity dataObject)
        {
            Name serviceModel = new()
            {
                LanguageId = dataObject.LanguageId,
                Value = dataObject.Value,
                Adjective = dataObject.Adjective,
                Comment = dataObject.Comment
            };

            return serviceModel;
        }

        internal static NameEntity ToDataObject(this Name serviceModel)
        {
            NameEntity dataObject = new()
            {
                LanguageId = serviceModel.LanguageId,
                Value = serviceModel.Value,
                Adjective = serviceModel.Adjective,
                Comment = serviceModel.Comment
            };

            return dataObject;
        }

        internal static IEnumerable<Name> ToServiceModels(this IEnumerable<NameEntity> dataObjects)
        {
            IEnumerable<Name> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }

        internal static IEnumerable<NameEntity> ToDataObjects(this IEnumerable<Name> serviceModels)
        {
            IEnumerable<NameEntity> dataObjects = serviceModels.Select(serviceModel => serviceModel.ToDataObject());

            return dataObjects;
        }
    }
}
