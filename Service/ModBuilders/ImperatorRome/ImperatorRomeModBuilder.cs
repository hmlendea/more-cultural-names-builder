using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.ImperatorRome
{
    public sealed class ImperatorRomeModBuilder : ModBuilder, IImperatorRomeModBuilder
    {
        public override string Game => "ImperatorRome";

        public ImperatorRomeModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(languageRepository, locationRepository, outputSettings)
        {
        }

        protected override void BuildMod()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, outputSettings.ImperatorRomeModId);
            string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "localization");
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
            string provinceNamesDirectoryPath = Path.Combine(commonDirectoryPath, "province_names");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(localisationDirectoryPath);
            Directory.CreateDirectory(provinceNamesDirectoryPath);

            Directory.CreateDirectory(commonDirectoryPath);

            CreateDataFiles(provinceNamesDirectoryPath);
            CreateLocalisationFiles(localisationDirectoryPath);
            CreateDescriptorFiles();
        }

        void CreateDataFiles(string provinceNamesDirectoryPath)
        {
            List<Localisation> localisations = GetLocalisations();

            foreach (string culture in localisations.Select(x => x.LanguageGameId).Distinct())
            {
                string path = Path.Combine(provinceNamesDirectoryPath, $"{culture.ToLower()}.txt");
                string content = $"{culture} = {{" + Environment.NewLine;

                foreach (Localisation localisation in localisations.Where(x => x.LanguageGameId == culture))
                {
                    content +=
                        $"    {localisation.LocationGameId} = PROV{localisation.LocationGameId}_{culture}" +
                        $" # {localisation.Name}" + Environment.NewLine;
                }

                content += "}";

                File.WriteAllText(path, content);
            }
        }

        void CreateLocalisationFiles(string localisationDirectoryPath)
        {
            CreateLocalisationFile(localisationDirectoryPath, "english");
            CreateLocalisationFile(localisationDirectoryPath, "french");
            CreateLocalisationFile(localisationDirectoryPath, "german");
            CreateLocalisationFile(localisationDirectoryPath, "spanish");
        }

        void CreateLocalisationFile(string localisationDirectoryPath, string language)
        {
            string fileContent = GenerateLocalisationFileContent(language);
            string fileName = $"{outputSettings.ImperatorRomeModId}_provincenames_l_{language}.yml";
            string filePath = Path.Combine(localisationDirectoryPath, fileName);

            File.WriteAllText(filePath, fileContent, Encoding.UTF8);
        }

        void CreateDescriptorFiles()
        {
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateInnerDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{outputSettings.ImperatorRomeModId}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, outputSettings.ImperatorRomeModId, $"descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }

        string GenerateLocalisationFileContent(string language)
        {
            List<Localisation> localisations = GetLocalisations();
            string content = $"l_{language}:{Environment.NewLine}";

            foreach(Localisation localisation in localisations)
            {
                content += $" PROV{localisation.LocationGameId}_{localisation.LanguageGameId}:0 \"{localisation.Name}\"{Environment.NewLine}";
            }

            return content;
        }

        string GenerateMainDescriptorContent()
        {
            return GenerateInnerDescriptorContent() + Environment.NewLine +
                $"path=\"mod/{outputSettings.ImperatorRomeModId}\"";
        }

        string GenerateInnerDescriptorContent()
        {
            return
                $"name=\"{outputSettings.ImperatorRomeModName}\"" + Environment.NewLine +
                $"version=\"{outputSettings.ModVersion}\"" + Environment.NewLine +
                $"supported_version=\"{outputSettings.CK3GameVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"}}";
        }
    }
}
