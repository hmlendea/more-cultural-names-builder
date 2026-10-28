using System.Xml.Serialization;

namespace MoreCulturalNamesBuilder.DataAccess.DataObjects
{
    [XmlType("GameId")]
    public sealed class GameIdDataObject
    {
        public GameIdDataObject() { }

        public GameIdDataObject(string game, string gameLocationId)
        {
            Game = game;
            Id = gameLocationId;
        }

        [XmlAttribute("game")]
        public string Game { get; set; } = null!;

        [XmlAttribute("type")]
        public string? Type { get; set; }

        [XmlAttribute("parent")]
        public string? Parent { get; set; }

        [XmlAttribute("defaultLanguage")]
        public string? DefaultNameLanguageId { get; set; }

        [XmlText]
        public string Id { get; set; } = null!;
    }
}
