using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        const int SpacesPerIdentationLevel = 4;

        public CK2ModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(languageRepository, locationRepository, outputSettings)
        {
            EncodingProvider encodingProvider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encodingProvider);
        }

        protected override void BuildMod()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, outputSettings.CK2HipModId);
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
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateInnerDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{outputSettings.CK2HipModId}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, outputSettings.CK2HipModId, "descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }

        string BuildLandedTitlesFile()
        {
            string content = ReadLandedTitlesFile(Path.Combine(ApplicationPaths.DataDirectory, "ck2hip_landed_titles.txt"));

            IEnumerable<Location> locations = locationRepository.GetAll().ToServiceModels();

            foreach (Location location in locations.Take(20))
            {
                Console.WriteLine(location.Id);
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

            string indentation = Regex.Match(landedTitlesFile, "^([ \t]*)" + gameId + "[ \t]*=[ \t]*\\{.*$", RegexOptions.Multiline).Groups[1].Value + "\t";
            string thisContent = string.Empty;

            foreach (Localisation localisation in localisations.OrderBy(x => x.LanguageId))
            {
                thisContent += $"{indentation}{localisation.LanguageId} = \"{localisation.Name}\"" + Environment.NewLine;
            }

            return Regex.Replace(landedTitlesFile, "^([ \t]*" + gameId + "[ \t]*=[ \t]*\\{.*$)", "$1\n" + thisContent, RegexOptions.Multiline);
        }

        string ReadLandedTitlesFile(string filePath)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            
            return File.ReadAllText(filePath, encoding);
        }

        void WriteLandedTitlesFile(string content, string landedTitlesDirectoryPath)
        {
            string filePath = Path.Combine(landedTitlesDirectoryPath, LandedTitlesFileName);

            Encoding encoding = Encoding.GetEncoding("windows-1252");
            byte[] contentBytes = encoding.GetBytes(content.ToCharArray());
            
            File.WriteAllBytes(filePath, contentBytes);
        }

        string GenerateMainDescriptorContent()
        {
            return GenerateInnerDescriptorContent() + Environment.NewLine +
                $"path = \"mod/{outputSettings.CK2HipModId}\"";
        }

        string GenerateInnerDescriptorContent()
        {
            return
                $"name = \"{outputSettings.CK2HipModName}\"" + Environment.NewLine +
                $"dependencies = {{ \"HIP - Historical Immersion Project\" }}" + Environment.NewLine +
                $"tags = {{ map immersion HIP }}";
        }
    }
}
