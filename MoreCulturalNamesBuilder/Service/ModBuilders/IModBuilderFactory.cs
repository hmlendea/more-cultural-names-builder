namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public interface IModBuilderFactory
    {
        IModBuilder GetModBuilder(string game);
    }
}