namespace MoreCulturalNamesModBuilder.Service
{
    public interface INameNormaliser
    {
        string ToHOI4(string name);
        
        string ToWindows1252(string name);
    }
}
