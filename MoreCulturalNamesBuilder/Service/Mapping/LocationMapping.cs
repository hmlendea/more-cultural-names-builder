using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    static class LocationMapping
    {
        internal static Location ToServiceModel(this LocationEntity dataObject)
        {
            Location serviceModel = new()
            {
                Id = dataObject.Id,
                GeoNamesId = dataObject.GeoNamesId,
                GameIds = dataObject.GameIds.ToServiceModels(),
                FallbackLocations = dataObject.FallbackLocations,
                Names = dataObject.Names.ToServiceModels()
            };

            return serviceModel;
        }

        internal static LocationEntity ToDataObject(this Location serviceModel)
        {
            LocationEntity dataObject = new()
            {
                Id = serviceModel.Id,
                GeoNamesId = serviceModel.GeoNamesId,
                GameIds = serviceModel.GameIds.ToDataObjects().ToList(),
                FallbackLocations = serviceModel.FallbackLocations.ToList(),
                Names = serviceModel.Names.ToDataObjects().ToList()
            };

            return dataObject;
        }

        internal static IEnumerable<Location> ToServiceModels(this IEnumerable<LocationEntity> dataObjects)
        {
            IEnumerable<Location> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }

        internal static IEnumerable<LocationEntity> ToDataObjects(this IEnumerable<Location> serviceModels)
        {
            IEnumerable<LocationEntity> dataObjects = serviceModels.Select(serviceModel => serviceModel.ToDataObject());

            return dataObjects;
        }
    }
}
