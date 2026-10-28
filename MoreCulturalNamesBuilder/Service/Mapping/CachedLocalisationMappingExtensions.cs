using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.Mapping
{
    internal static class CachedLocalisationMappingExtensions
    {
        internal static Localisation? ToServiceModel(this CachedLocalisation cachedLocalisation)
        {
            if (!cachedLocalisation.HasValue)
            {
                return null;
            }

            return new()
            {
                Id = cachedLocalisation.LocationId!,
                LanguageId = cachedLocalisation.LanguageId!,
                Name = cachedLocalisation.Name!,
                Adjective = cachedLocalisation.Adjective,
                Comment = cachedLocalisation.Comment
            };
        }
    }
}
