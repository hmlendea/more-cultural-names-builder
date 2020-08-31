using System.Collections.Generic;
using System.Xml.Serialization;

using NuciDAL.DataObjects;

namespace MoreCulturalNamesModBuilder.DataAccess.DataObjects
{
    public class LocationEntity : EntityBase
    {
        public string GeoNamesId { get; set; }

        [XmlArrayItem("LocationId")]
        public List<string> FallbackLocations { get; set; }

        public List<GameIdEntity> GameIds { get; set; }

        public List<LocationNameEntity> Names { get; set; }
    }
}
