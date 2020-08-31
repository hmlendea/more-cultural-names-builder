using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.Mapping
{
    static class GameIdMapping
    {
        internal static GameId ToServiceModel(this GameIdEntity dataObject)
        {
            GameId serviceModel = new GameId();
            serviceModel.Game = dataObject.Game;
            serviceModel.ParentId = dataObject.ParentId;
            serviceModel.Id = dataObject.Id;

            return serviceModel;
        }

        internal static GameIdEntity ToDataObject(this GameId serviceModel)
        {
            GameIdEntity dataObject = new GameIdEntity();
            dataObject.Game = serviceModel.Game;
            dataObject.ParentId = serviceModel.ParentId;
            dataObject.Id = serviceModel.Id;

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
