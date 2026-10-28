using System;

namespace MoreCulturalNamesBuilder.Service.Models
{
    public sealed class GameLocationKey(string game, string id) : IEquatable<GameLocationKey>
    {
        public string Game { get; set; } = game;

        public string Id { get; set; } = id;

        public bool Equals(GameLocationKey? other)
            => other is not null && string.Equals(Game, other.Game) && string.Equals(Id, other.Id);

        public override bool Equals(object? other) => Equals(other as GameLocationKey);

        public override int GetHashCode() => HashCode.Combine(Game, Id);
    }
}
