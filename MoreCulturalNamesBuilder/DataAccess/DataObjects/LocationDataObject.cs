using System.Collections.Generic;
using System.Xml.Serialization;

using NuciDAL.DataObjects;

namespace MoreCulturalNamesBuilder.DataAccess.DataObjects
{
    [XmlType("LocationEntity")]
    public sealed class LocationDataObject : EntityBase
    {
        public string? GeoNamesId { get; set; }

        [XmlArrayItem("LocationId")]
        public List<string> FallbackLocations { get; set; } = null!;

        public List<GameIdDataObject> GameIds { get; set; } = null!;

        public List<NameDataObject> Names { get; set; } = null!;
    }
}
