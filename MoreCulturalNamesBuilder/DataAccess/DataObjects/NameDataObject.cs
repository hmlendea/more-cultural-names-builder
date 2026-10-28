using System.Xml.Serialization;

namespace MoreCulturalNamesBuilder.DataAccess.DataObjects
{
    [XmlType("Name")]
    public sealed class NameDataObject
    {
        public NameDataObject() { }

        public NameDataObject(string language, string value)
        {
            LanguageId = language;
            Value = value;
        }

        [XmlAttribute("language")]
        public string LanguageId { get; set; } = null!;

        [XmlAttribute("value")]
        public string Value { get; set; } = null!;

        [XmlAttribute("adjective")]
        public string? Adjective { get; set; }

        [XmlAttribute("comment")]
        public string? Comment { get; set; }
    }
}
