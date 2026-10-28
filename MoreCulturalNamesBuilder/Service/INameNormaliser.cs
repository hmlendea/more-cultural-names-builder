namespace MoreCulturalNamesBuilder.Service
{
    public interface INameNormaliser
    {
        public string ToCK3Charset(string name);

        public string ToHOI4CityCharset(string name);

        public string ToHOI4StateCharset(string name);

        public string ToImperatorRomeCharset(string name);

        public string ToWindows1252(string name);
    }
}
