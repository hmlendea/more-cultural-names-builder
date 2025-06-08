using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    static class LocationMapping
    {
        internal static Location ToServiceModel(this LocationEntity dataObject) => new()
        {
            Id = dataObject.Id,
            GeoNamesId = dataObject.GeoNamesId,
            GameIds = dataObject.GameIds.ToServiceModels(),
            FallbackLocations = dataObject.FallbackLocations,
            Names = dataObject.Names.ToServiceModels()
        };

        internal static LocationEntity ToDataObject(this Location serviceModel) => new()
        {
            Id = serviceModel.Id,
            GeoNamesId = serviceModel.GeoNamesId,
            GameIds = serviceModel.GameIds.ToDataObjects().ToList(),
            FallbackLocations = serviceModel.FallbackLocations.ToList(),
            Names = serviceModel.Names.ToDataObjects().ToList()
        };

        internal static IEnumerable<Location> ToServiceModels(this IEnumerable<LocationEntity> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        internal static IEnumerable<LocationEntity> ToDataObjects(this IEnumerable<Location> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
