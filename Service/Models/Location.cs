using System.Collections.Generic;
using System.Linq;

namespace MoreCulturalNamesModBuilder.Service.Models
{
    public sealed class Location
    {
        public string Id { get; set; }

        public string GeoNamesId { get; set; }

        public IEnumerable<GameId> GameIds { get; set; }

        public IEnumerable<string> FallbackLocations { get; set; }

        public IEnumerable<LocationName> Names { get; set; }

        public bool IsEmpty() => Names.Count() == 0 && FallbackLocations.Count() == 0;
    }
}
