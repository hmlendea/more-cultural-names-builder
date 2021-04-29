using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.Mapping
{
    static class TitleMapping
    {
        internal static Title ToServiceModel(this TitleEntity dataObject)
        {
            Title serviceModel = new Title();
            serviceModel.Id = dataObject.Id;
            serviceModel.GameIds = dataObject.GameIds.ToServiceModels();
            serviceModel.FallbackTitles = dataObject.FallbackTitles;
            serviceModel.Names = dataObject.Names.ToServiceModels();

            return serviceModel;
        }

        internal static TitleEntity ToDataObject(this Title serviceModel)
        {
            TitleEntity dataObject = new TitleEntity();
            dataObject.Id = serviceModel.Id;
            dataObject.GameIds = serviceModel.GameIds.ToDataObjects().ToList();
            dataObject.FallbackTitles = serviceModel.FallbackTitles.ToList();
            dataObject.Names = serviceModel.Names.ToDataObjects().ToList();

            return dataObject;
        }

        internal static IEnumerable<Title> ToServiceModels(this IEnumerable<TitleEntity> dataObjects)
        {
            IEnumerable<Title> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }

        internal static IEnumerable<TitleEntity> ToDataObjects(this IEnumerable<Title> serviceModels)
        {
            IEnumerable<TitleEntity> dataObjects = serviceModels.Select(serviceModel => serviceModel.ToDataObject());

            return dataObjects;
        }
    }
}
