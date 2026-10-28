using System.Collections.Generic;
using System.Linq;

namespace MoreCulturalNamesBuilder.Service.Models
{
    public sealed class Location
    {
        public string Id { get; set; } = null!;

        public string? GeoNamesId { get; set; }

        public IEnumerable<GameId> GameIds { get; set; } = null!;

        public IEnumerable<string> FallbackLocations { get; set; } = null!;

        public IEnumerable<Name> Names { get; set; } = null!;

        public bool IsEmpty() => !Names.Any() && !FallbackLocations.Any();
    }
}
