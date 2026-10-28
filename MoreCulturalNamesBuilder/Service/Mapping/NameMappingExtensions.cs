using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    internal static class NameMappingExtensions
    {
        internal static Name ToServiceModel(this NameDataObject dataObject) => new()
        {
            LanguageId = dataObject.LanguageId,
            Value = dataObject.Value,
            Adjective = dataObject.Adjective,
            Comment = dataObject.Comment
        };

        internal static NameDataObject ToDataObject(this Name serviceModel) => new()
        {
            LanguageId = serviceModel.LanguageId,
            Value = serviceModel.Value,
            Adjective = serviceModel.Adjective,
            Comment = serviceModel.Comment
        };

        internal static IEnumerable<Name> ToServiceModels(this IEnumerable<NameDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        internal static IEnumerable<NameDataObject> ToDataObjects(this IEnumerable<Name> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
