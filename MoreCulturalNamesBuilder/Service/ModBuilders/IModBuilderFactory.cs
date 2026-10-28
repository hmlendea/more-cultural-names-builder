using MoreCulturalNamesBuilder.Configuration;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public interface IModBuilderFactory
    {
        public IModBuilder GetModBuilder(Settings settings);
    }
}
