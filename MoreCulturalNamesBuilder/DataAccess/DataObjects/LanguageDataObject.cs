using System.Collections.Generic;
using System.Xml.Serialization;

using NuciDAL.DataObjects;

namespace MoreCulturalNamesBuilder.DataAccess.DataObjects
{
    [XmlType("Language")]
    public sealed class LanguageDataObject : EntityBase
    {
        public LanguageCodeDataObject? Code { get; set; }

        public List<GameIdDataObject> GameIds { get; set; } = null!;

        [XmlArrayItem(ElementName = "LanguageId")]
        public List<string> FallbackLanguages { get; set; } = null!;
    }
}
