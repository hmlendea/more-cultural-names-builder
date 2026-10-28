namespace MoreCulturalNamesBuilder.Service.Models
{
    public sealed class Name
    {
        public string LanguageId { get; set; } = null!;

        public string Value { get; set; } = null!;

        public string? Adjective { get; set; }

        public string? Comment { get; set; }
    }
}
