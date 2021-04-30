using System.Xml.Serialization;

namespace MoreCulturalNamesModBuilder.DataAccess.DataObjects
{
    [XmlType("GameId")]
    public class GameIdEntity
    {
        public GameIdEntity()
        {

        }

        public GameIdEntity(string game, string gameLocationId)
        {
            Game = game;
            Id = gameLocationId;
        }
        
        [XmlAttribute("game")]
        public string Game { get; set; }
        
        [XmlAttribute("type")]
        public string Type { get; set; }
        
        [XmlAttribute("parent")]
        public string Parent { get; set; }

        [XmlText]
        public string Id { get; set; }
    }
}
