namespace MoreCulturalNamesModBuilder.Service
{
    public interface INameNormaliser
    {
        string ToHOI4Charset(string name);
        
        string ToCK3Charset(string name);
        
        string ToWindows1252(string name);
    }
}
