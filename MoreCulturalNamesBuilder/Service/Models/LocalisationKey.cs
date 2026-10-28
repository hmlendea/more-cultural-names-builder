using System;

namespace MoreCulturalNamesBuilder.Service.Models
{
    public sealed class LocalisationKey(string locationId, string languageId) : IEquatable<LocalisationKey>
    {
        public string LocationId { get; set; } = locationId;

        public string LanguageId { get; set; } = languageId;

        public bool Equals(LocalisationKey? other) =>
            other is not null &&
            string.Equals(LocationId, other.LocationId) &&
            string.Equals(LanguageId, other.LanguageId);

        public override bool Equals(object? other) => Equals(other as LocalisationKey);

        public override int GetHashCode() => HashCode.Combine(LocationId, LanguageId);
    }
}
