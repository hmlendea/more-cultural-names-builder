using System;

namespace MoreCulturalNamesBuilder.Service.Models
{
    public sealed class TypedGameLocationKey(string game, string id, string? type) : IEquatable<TypedGameLocationKey>
    {
        public string Game { get; set; } = game;

        public string Id { get; set; } = id;

        public string? Type { get; set; } = type;

        public bool Equals(TypedGameLocationKey? other) =>
            other is not null &&
            string.Equals(Game, other.Game) &&
            string.Equals(Id, other.Id) &&
            string.Equals(Type, other.Type);

        public override bool Equals(object? other) => Equals(other as TypedGameLocationKey);

        public override int GetHashCode() => HashCode.Combine(Game, Id, Type);
    }
}
