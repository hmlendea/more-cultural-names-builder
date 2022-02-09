using MoreCulturalNamesBuilder.Configuration;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public interface IModBuilderFactory
    {
        IModBuilder GetModBuilder(Settings settings);
    }
}