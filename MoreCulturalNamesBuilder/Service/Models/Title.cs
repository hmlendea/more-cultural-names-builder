using System.Collections.Generic;
using System.Linq;

namespace MoreCulturalNamesBuilder.Service.Models
{
    public sealed class Title
    {
        public string Id { get; set; }

        public IEnumerable<GameId> GameIds { get; set; }

        public IEnumerable<string> FallbackTitles { get; set; }

        public IEnumerable<Name> Names { get; set; }

        public bool IsEmpty() => Names.Count() == 0 && FallbackTitles.Count() == 0;
    }
}
