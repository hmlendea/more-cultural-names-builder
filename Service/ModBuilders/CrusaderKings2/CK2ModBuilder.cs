using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Mapping;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings2
{
    public sealed class CK2ModBuilder : ModBuilder, ICK2ModBuilder
    {
        public override string Game => "CK2HIP";

        const string LandedTitlesFileName = "0_HIP_MoreCulturalNames.txt";

        public CK2ModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(languageRepository, locationRepository, outputSettings)
        {
        }

        public override void Build()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, outputSettings.CK2HipModId);
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
            string landedTitlesDirectoryPath = Path.Combine(commonDirectoryPath, "landed_titles");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(landedTitlesDirectoryPath);

            Directory.CreateDirectory(commonDirectoryPath);

            CreateDataFiles(landedTitlesDirectoryPath);

            List<Localisation> localisations = GetLocalisations();
        }

        void CreateDataFiles(string landedTitlesDirectoryPath)
        {
            List<Localisation> localisations = GetLocalisations();

            string path = Path.Combine(landedTitlesDirectoryPath, LandedTitlesFileName);
            string content = string.Empty;

            Dictionary<string, List<Localisation>> localisationsByLocation = localisations
                .GroupBy(x => x.LocationId)
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (string locationId in localisationsByLocation.Keys)
            {
                content += $"{locationId} = {{" + Environment.NewLine;

                foreach (Localisation localisation in localisationsByLocation[locationId])
                {
                    content += $"    {localisation.LanguageId} = \"{localisation.Name}\"" + Environment.NewLine;
                }

                content += $"}}" + Environment.NewLine;
            }

            File.WriteAllText(path, content);
        }
    }
}
