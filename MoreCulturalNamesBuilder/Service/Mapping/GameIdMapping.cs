using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    static class GameIdMapping
    {
        internal static GameId ToServiceModel(this GameIdEntity dataObject) => new()
        {
            Game = dataObject.Game,
            Type = dataObject.Type,
            Parent = dataObject.Parent,
            DefaultNameLanguageId = dataObject.DefaultNameLanguageId,
            Id = dataObject.Id
        };

        internal static GameIdEntity ToDataObject(this GameId serviceModel) => new()
        {
            Game = serviceModel.Game,
            Type = serviceModel.Type,
            Parent = serviceModel.Parent,
            DefaultNameLanguageId = serviceModel.DefaultNameLanguageId,
            Id = serviceModel.Id
        };

        internal static IEnumerable<GameId> ToServiceModels(this IEnumerable<GameIdEntity> dataObjects)
            => dataObjects.Select(dataObject => dataObject.ToServiceModel());

        internal static IEnumerable<GameIdEntity> ToDataObjects(this IEnumerable<GameId> serviceModels)
            => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
    }
}
