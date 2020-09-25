using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings2
{
    public sealed class CK2HIPModBuilder : CK2ModBuilder, ICK2HIPModBuilder
    {
        public override string Game => "CK2HIP";

        protected override string InputLandedTitlesFileName => "ck2hip_landed_titles.txt";

        protected override string OutputLandedTitlesFileName => "swmh_landed_titles.txt";

        public CK2HIPModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(languageRepository, locationRepository, outputSettings)
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
                $"name = \"{outputSettings.CK2HipModName}\"" + Environment.NewLine +
                $"dependencies = {{ \"HIP - Historical Immersion Project\" }}" + Environment.NewLine +
                $"tags = {{ map immersion HIP }}";
        }
    }
}
