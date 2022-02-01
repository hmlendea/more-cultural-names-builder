namespace MoreCulturalNamesBuilder.Service
{
    public interface INameNormaliser
    {
        string ToCK3Charset(string name);
        
        string ToHOI4CityCharset(string name);
        
        string ToHOI4StateCharset(string name);
        
        string ToImperatorRomeCharset(string name);
        
        string ToWindows1252(string name);
    }
}
