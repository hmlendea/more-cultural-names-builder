namespace MoreCulturalNamesBuilder.Service.Models
{
    public sealed class Localisation
    {
        public string Id { get; set; } = null!;

        public string GameId { get; set; } = null!;

        public string LanguageId { get; set; } = null!;

        public string LanguageGameId { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Adjective { get; set; }

        public string? Comment { get; set; }
    }
}
