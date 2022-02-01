using System.Collections.Generic;
using System.Xml.Serialization;

using NuciDAL.DataObjects;

namespace MoreCulturalNamesBuilder.DataAccess.DataObjects
{
    public class TitleEntity : EntityBase
    {
        [XmlArrayItem("TitleId")]
        public List<string> FallbackTitles { get; set; }

        public List<GameIdEntity> GameIds { get; set; }

        public List<NameEntity> Names { get; set; }
    }
}
