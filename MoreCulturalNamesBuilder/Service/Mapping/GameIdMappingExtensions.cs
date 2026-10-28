using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    internal static class GameIdMappingExtensions
    {
        internal static GameId ToServiceModel(this GameIdDataObject dataObject) => new()
        {
            Game = dataObject.Game,
            Type = dataObject.Type,
            Parent = dataObject.Parent,
            DefaultNameLanguageId = dataObject.DefaultNameLanguageId,
            Id = dataObject.Id
        };

        internal static GameIdDataObject ToDataObject(this GameId serviceModel) => new()
        {
            Game = serviceModel.Game,
            Type = serviceModel.Type,
            Parent = serviceModel.Parent,
            DefaultNameLanguageId = serviceModel.DefaultNameLanguageId,
            Id = serviceModel.Id
        };

        internal static IEnumerable<GameId> ToServiceModels(this IEnumerable<GameIdDataObject> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        internal static IEnumerable<GameIdDataObject> ToDataObjects(this IEnumerable<GameId> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
