using System.Xml.Serialization;

namespace MoreCulturalNamesModBuilder.DataAccess.DataObjects
{
    [XmlType("Name")]
    public class NameEntity
    {
        public NameEntity()
        {

        }

        public NameEntity(string language, string value)
        {
            LanguageId = language;
            Value = value;
        }
        
        [XmlAttribute("language")]
        public string LanguageId { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("adjective")]
        public string Adjective { get; set; }

        [XmlAttribute("comment")]
        public string Comment { get; set; }
    }
}
