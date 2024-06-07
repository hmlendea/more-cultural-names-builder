using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    static class GameIdMapping
    {
        internal static GameId ToServiceModel(this GameIdEntity dataObject)
        {
            GameId serviceModel = new()
            {
                Game = dataObject.Game,
                Type = dataObject.Type,
                Parent = dataObject.Parent,
                DefaultNameLanguageId = dataObject.DefaultNameLanguageId,
                Id = dataObject.Id
            };

            return serviceModel;
        }

        internal static GameIdEntity ToDataObject(this GameId serviceModel)
        {
            GameIdEntity dataObject = new()
            {
                Game = serviceModel.Game,
                Type = serviceModel.Type,
                Parent = serviceModel.Parent,
                DefaultNameLanguageId = serviceModel.DefaultNameLanguageId,
                Id = serviceModel.Id
            };

            return dataObject;
        }

        internal static IEnumerable<GameId> ToServiceModels(this IEnumerable<GameIdEntity> dataObjects)
        {
            IEnumerable<GameId> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }

        internal static IEnumerable<GameIdEntity> ToDataObjects(this IEnumerable<GameId> serviceModels)
        {
            IEnumerable<GameIdEntity> dataObjects = serviceModels.Select(serviceModel => serviceModel.ToDataObject());

            return dataObjects;
        }
    }
}
