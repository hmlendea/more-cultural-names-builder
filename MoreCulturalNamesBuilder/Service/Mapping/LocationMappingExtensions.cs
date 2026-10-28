using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    internal static class LocationMappingExtensions
    {
        internal static Location ToServiceModel(this LocationDataObject dataObject) => new()
        {
            Id = dataObject.Id,
            GeoNamesId = dataObject.GeoNamesId,
            GameIds = dataObject.GameIds.ToServiceModels(),
            FallbackLocations = dataObject.FallbackLocations,
            Names = dataObject.Names.ToServiceModels()
        };

        internal static LocationDataObject ToDataObject(this Location serviceModel) => new()
        {
            Id = serviceModel.Id,
            GeoNamesId = serviceModel.GeoNamesId,
            GameIds = [.. serviceModel.GameIds.ToDataObjects()],
            FallbackLocations = [.. serviceModel.FallbackLocations],
            Names = [.. serviceModel.Names.ToDataObjects()]
        };

        internal static IEnumerable<Location> ToServiceModels(this IEnumerable<LocationDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        internal static IEnumerable<LocationDataObject> ToDataObjects(this IEnumerable<Location> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
