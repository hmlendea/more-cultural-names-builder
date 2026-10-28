namespace MoreCulturalNamesBuilder.Service.Models
{
    public sealed class CachedLocalisation(
        bool hasValue,
        string? locationId,
        string? languageId,
        string? name,
        string? adjective,
        string? comment)
    {
        public bool HasValue { get; set; } = hasValue;

        public string? LocationId { get; set; } = locationId;

        public string? LanguageId { get; set; } = languageId;

        public string? Name { get; set; } = name;

        public string? Adjective { get; set; } = adjective;

        public string? Comment { get; set; } = comment;
    }
}
