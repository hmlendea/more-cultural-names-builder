using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    static class NameMapping
    {
        internal static Name ToServiceModel(this NameEntity dataObject) => new()
        {
            LanguageId = dataObject.LanguageId,
            Value = dataObject.Value,
            Adjective = dataObject.Adjective,
            Comment = dataObject.Comment
        };

        internal static NameEntity ToDataObject(this Name serviceModel) => new()
        {
            LanguageId = serviceModel.LanguageId,
            Value = serviceModel.Value,
            Adjective = serviceModel.Adjective,
            Comment = serviceModel.Comment
        };

        internal static IEnumerable<Name> ToServiceModels(this IEnumerable<NameEntity> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        internal static IEnumerable<NameEntity> ToDataObjects(this IEnumerable<Name> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
