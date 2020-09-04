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

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings3
{
    public sealed class CK3ModBuilder : ModBuilder, ICK3ModBuilder
    {
        public override string Game => "CK3";

        const string LandedTitlesFileName = "999_MoreCulturalNames.txt";
        const int SpacesPerIdentationLevel = 4;
        const string NewLine = "\r\n";

        public CK3ModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(languageRepository, locationRepository, outputSettings)
        {
        }

        protected override void BuildMod()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, outputSettings.CK3ModId);
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
            string landedTitlesDirectoryPath = Path.Combine(commonDirectoryPath, "landed_titles");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(landedTitlesDirectoryPath);

            Directory.CreateDirectory(commonDirectoryPath);

            CreateDataFiles(landedTitlesDirectoryPath);
            CreateDescriptorFiles();
        }

        void CreateDataFiles(string landedTitlesDirectoryPath)
        {
            List<Localisation> localisations = GetLocalisations();

            Dictionary<string, List<Localisation>> localisationsByLocation = localisations
                .GroupBy(x => x.LocationId)
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.ToList());

            IEnumerable<Location> locations = locationRepository.GetAll().ToServiceModels();
            
            string content = BuildLandedTitlesFile();
            WriteLandedTitlesFile(content, landedTitlesDirectoryPath);
        }

        void CreateDescriptorFiles()
        {
            string fileContent = GenerateDescriptorFileContent();

            string descriptorFile1Path = Path.Combine(OutputDirectoryPath, $"{outputSettings.CK3ModId}.mod");
            string descriptorFile2Path = Path.Combine(OutputDirectoryPath, outputSettings.CK3ModId, "descriptor.mod");

            File.WriteAllText(descriptorFile1Path, fileContent);
            File.WriteAllText(descriptorFile2Path, fileContent);
        }

        string BuildLandedTitlesFile()
        {
            string content = File.ReadAllText("Data/ck3_landed_titles.txt");

            IEnumerable<Location> locations = locationRepository.GetAll().ToServiceModels();

            foreach (Location location in locations)
            {
                foreach (GameId gameLocation in location.GameIds.Where(x => x.Game == Game))
                {
                    content = AddLocalisationsToTitle(content, gameLocation.Id);
                }
            }

            return content;
        }

        string AddLocalisationsToTitle(string landedTitlesFile, string gameId)
        {
            IList<Localisation> localisations = GetGameLocationLocalisations(gameId);

            if (!localisations.Any())
            {
                return landedTitlesFile;
            }

            string indentation1 = Regex.Match(landedTitlesFile, "^([ \t]*)" + gameId + "[ \t]*=[ \t]*\\{.*$", RegexOptions.Multiline).Groups[1].Value;
            string indentation2 = indentation1 + "\t";
            string indentation3 = indentation2 + "\t";
            string thisContent = string.Empty;

            thisContent += $"{indentation2}cultural_names = {{" + NewLine;

            foreach (Localisation localisation in localisations)
            {
                thisContent += $"{indentation3}{localisation.LanguageId} = \"{localisation.Name}\"" + NewLine;
            }

            thisContent += $"{indentation2}}}" + NewLine;

            return Regex.Replace(landedTitlesFile, "^([ \t]*" + gameId + "[ \t]*=[ \t]*\\{.*$)", "$1\n" + thisContent, RegexOptions.Multiline);
        }

        void WriteLandedTitlesFile(string content, string landedTitlesDirectoryPath)
        {
            string filePath = Path.Combine(landedTitlesDirectoryPath, LandedTitlesFileName);

            File.WriteAllText(filePath, content);
        }

        string GenerateDescriptorFileContent()
        {
            return
                $"version = 1.0" + NewLine +
                $"tags = {{" + NewLine +
                $"    \"Culture\"" + NewLine +
                $"    \"Historical\"" + NewLine +
                $"    \"Map\"" + NewLine +
                $"    \"Translation\"" + NewLine +
                $"}}" + NewLine +
                $"name = \"{outputSettings.CK3ModName}\"" + NewLine +
                $"supported_version = \"1.0.*\"" + NewLine +
                $"path = \"mod/{outputSettings.CK3ModId}\"" + NewLine +
                $"picture = \"mcn.png\"";
        }
    }
}
