using System;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings2
{
    public sealed class CK2HIPModBuilder : CK2ModBuilder, ICK2HIPModBuilder
    {
        protected override string InputLandedTitlesFileName => "ck2hip_landed_titles.txt";
        protected override string OutputLandedTitlesFileName => "swmh_landed_titles.txt";

        public CK2HIPModBuilder(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            IRepository<TitleEntity> titleRepository,
            BuildSettings buildSettings,
            OutputSettings outputSettings)
            : base(
                localisationFetcher,
                nameNormaliser,
                languageRepository,
                locationRepository,
                titleRepository,
                buildSettings,
                outputSettings)
        {
        }

        protected override string GenerateMainDescriptorContent()
        {
            return GenerateDescriptorContent() + Environment.NewLine +
                $"path = \"mod/{outputSettings.CK2HipModId}\"";
        }

        protected override string GenerateDescriptorContent()
        {
            return
                $"# Version {outputSettings.ModVersion} ({DateTime.Now})" + Environment.NewLine +
                $"name = \"{outputSettings.CK2HipModName}\"" + Environment.NewLine +
                $"dependencies = {{ \"HIP - Historical Immersion Project\" }}" + Environment.NewLine +
                $"picture = \"thumbnail.png\"" + Environment.NewLine +
                $"tags = {{ map immersion HIP }}";
        }
    }
}
