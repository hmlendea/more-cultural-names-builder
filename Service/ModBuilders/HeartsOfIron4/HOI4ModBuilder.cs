using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Mapping;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.HeartsOfIron4
{
    public sealed class HOI4ModBuilder : ModBuilder, IHOI4ModBuilder
    {
        public override string Game => "HOI4";

        public HOI4ModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(languageRepository, locationRepository, outputSettings)
        {
        }

        protected override void BuildMod()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, outputSettings.HOI4ModId);
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(commonDirectoryPath);

            CreateDescriptorFiles();
        }

        void CreateDescriptorFiles()
        {
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateInnerDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{outputSettings.HOI4ModId}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, outputSettings.HOI4ModId, $"descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }
        
        string GenerateMainDescriptorContent()
        {
            return GenerateInnerDescriptorContent() + Environment.NewLine +
                $"path=\"mod/{outputSettings.HOI4ModId}\"";
        }

        string GenerateInnerDescriptorContent()
        {
            return
                $"name=\"{outputSettings.HOI4ModName}\"" + Environment.NewLine +
                $"version=\"{outputSettings.ModVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"    \"Map\"" + Environment.NewLine +
                $"    \"Translation\"" + Environment.NewLine +
                $"}}" + Environment.NewLine +
                $"supported_version=\"{outputSettings.HOI4GameVersion}\"";
        }
    }
}
