using System.Collections.Generic;

namespace MoreCulturalNamesBuilder.Service.Models
{
    public sealed class Language
    {
        public string Id { get; set; } = null!;

        public LanguageCode? Code { get; set; }

        public IEnumerable<GameId> GameIds { get; set; } = null!;

        public IEnumerable<string> FallbackLanguages { get; set; } = null!;
    }
}
