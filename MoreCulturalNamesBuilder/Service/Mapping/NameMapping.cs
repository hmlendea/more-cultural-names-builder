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
            Name serviceModel = new Name();
            serviceModel.LanguageId = dataObject.LanguageId;
            serviceModel.Value = dataObject.Value;
            serviceModel.Adjective = dataObject.Adjective;
            serviceModel.Comment = dataObject.Comment;

            return serviceModel;
        }

        internal static NameEntity ToDataObject(this Name serviceModel)
        {
            NameEntity dataObject = new NameEntity();
            dataObject.LanguageId = serviceModel.LanguageId;
            dataObject.Value = serviceModel.Value;
            dataObject.Adjective = serviceModel.Adjective;
            dataObject.Comment = serviceModel.Comment;

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
